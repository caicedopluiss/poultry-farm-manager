using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Application.Commands.FeedingTables;

public sealed class CreateFeedingTableCommand
{
    public record Args(NewFeedingTableDto NewFeedingTable);
    public record Result(FeedingTableDto CreatedFeedingTable);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var feedingTable = args.NewFeedingTable.Map();
            var created = await unitOfWork.FeedingTables.CreateAsync(feedingTable, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new Result(new FeedingTableDto().Map(created));
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string, string)>();

            var trimmedName = args.NewFeedingTable.Name?.Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                errors.Add(("name", "Name is required."));
            }
            else if (trimmedName.Length > 100)
            {
                errors.Add(("name", "Name cannot exceed 100 characters."));
            }
            else
            {
                var existing = await unitOfWork.FeedingTables.GetByNameAsync(trimmedName, cancellationToken);
                if (existing != null)
                {
                    errors.Add(("name", "A feeding table with this name already exists."));
                }
            }

            if (args.NewFeedingTable.Description?.Length > 500)
            {
                errors.Add(("description", "Description cannot exceed 500 characters."));
            }

            if (args.NewFeedingTable.DayEntries == null || args.NewFeedingTable.DayEntries.Count == 0)
            {
                errors.Add(("dayEntries", "At least one day entry is required."));
            }
            else
            {
                var dayNumbers = args.NewFeedingTable.DayEntries.Select(e => e.DayNumber).ToList();

                if (dayNumbers.Any(d => d < 1))
                {
                    errors.Add(("dayEntries", "Day numbers must be greater than or equal to 1."));
                }

                if (dayNumbers.Count != dayNumbers.Distinct().Count())
                {
                    errors.Add(("dayEntries", "Duplicate day numbers are not allowed."));
                }

                if (args.NewFeedingTable.DayEntries.Any(e => e.AmountPerBird <= 0))
                {
                    errors.Add(("dayEntries", "Amount must be greater than 0."));
                }

                var validWeightUnits = new[] { UnitOfMeasure.Kilogram, UnitOfMeasure.Gram, UnitOfMeasure.Pound };
                foreach (var entry in args.NewFeedingTable.DayEntries)
                {
                    if (!Enum.TryParse<FoodType>(entry.FoodType, ignoreCase: true, out _))
                    {
                        errors.Add(("dayEntries", $"Invalid food type: '{entry.FoodType}'."));
                    }

                    if (string.IsNullOrWhiteSpace(entry.UnitOfMeasure))
                    {
                        errors.Add(("dayEntries", "Unit of measure is required."));
                    }
                    else if (!Enum.TryParse<UnitOfMeasure>(entry.UnitOfMeasure, ignoreCase: true, out var unitOfMeasure))
                    {
                        errors.Add(("dayEntries", $"Invalid unit of measure: '{entry.UnitOfMeasure}'."));
                    }
                    else if (!validWeightUnits.Contains(unitOfMeasure))
                    {
                        errors.Add(("dayEntries", $"Invalid unit of measure: '{entry.UnitOfMeasure}'. Only weight units (Kilogram, Gram, Pound) are allowed."));
                    }

                    if (entry.ExpectedBirdWeight.HasValue)
                    {
                        if (entry.ExpectedBirdWeight.Value <= 0)
                        {
                            errors.Add(("dayEntries", "Expected bird weight must be greater than 0."));
                        }
                        if (string.IsNullOrWhiteSpace(entry.ExpectedBirdWeightUnitOfMeasure))
                        {
                            errors.Add(("dayEntries", "Expected bird weight unit of measure is required when expected bird weight is set."));
                        }
                        else if (!Enum.TryParse<UnitOfMeasure>(entry.ExpectedBirdWeightUnitOfMeasure, ignoreCase: true, out var weightUom) || !validWeightUnits.Contains(weightUom))
                        {
                            errors.Add(("dayEntries", $"Invalid expected bird weight unit of measure: '{entry.ExpectedBirdWeightUnitOfMeasure}'. Only weight units (Kilogram, Gram, Pound) are allowed."));
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(entry.ExpectedBirdWeightUnitOfMeasure))
                    {
                        errors.Add(("dayEntries", "Expected bird weight unit of measure cannot be set without an expected bird weight."));
                    }
                }
            }

            return errors;
        }
    }
}

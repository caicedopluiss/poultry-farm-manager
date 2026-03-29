using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Application.Commands.FeedingTables;

public sealed class UpdateFeedingTableCommand
{
    public record Args(Guid FeedingTableId, UpdateFeedingTableDto Updates);
    public record Result(FeedingTableDto UpdatedFeedingTable);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        private FeedingTable? feedingTable;

        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var updates = args.Updates;

            if (!string.IsNullOrWhiteSpace(updates.Name))
            {
                feedingTable!.Name = updates.Name.Trim();
            }

            if (updates.Description != null)
            {
                feedingTable!.Description = string.IsNullOrWhiteSpace(updates.Description) ? null : updates.Description.Trim();
            }

            if (updates.DayEntries != null)
            {
                unitOfWork.FeedingTables.DeleteDayEntries(feedingTable!.DayEntries);
                feedingTable!.DayEntries = updates.DayEntries.Select(e => e.Map()).ToList();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new Result(new FeedingTableDto().Map(feedingTable!));
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string, string)>();

            if (args.FeedingTableId == Guid.Empty)
            {
                errors.Add(("feedingTableId", "Feeding table ID is required."));
                return errors;
            }

            feedingTable = await unitOfWork.FeedingTables.GetByIdAsync(args.FeedingTableId, track: true, ct: cancellationToken);
            if (feedingTable == null)
            {
                errors.Add(("feedingTableId", $"Feeding table with ID {args.FeedingTableId} not found."));
                return errors;
            }

            if (args.Updates.Name != null)
            {
                var trimmedName = args.Updates.Name.Trim();
                if (string.IsNullOrWhiteSpace(trimmedName))
                {
                    errors.Add(("name", "Name cannot be empty."));
                }
                else if (trimmedName.Length > 100)
                {
                    errors.Add(("name", "Name cannot exceed 100 characters."));
                }
                else
                {
                    var existing = await unitOfWork.FeedingTables.GetByNameAsync(trimmedName, cancellationToken);
                    if (existing != null && existing.Id != args.FeedingTableId)
                    {
                        errors.Add(("name", "A feeding table with this name already exists."));
                    }
                }
            }

            if (args.Updates.Description?.Length > 500)
            {
                errors.Add(("description", "Description cannot exceed 500 characters."));
            }

            if (args.Updates.DayEntries != null)
            {
                if (args.Updates.DayEntries.Count == 0)
                {
                    errors.Add(("dayEntries", "At least one day entry is required."));
                }
                else
                {
                    var dayNumbers = args.Updates.DayEntries.Select(e => e.DayNumber).ToList();

                    if (dayNumbers.Any(d => d < 1))
                    {
                        errors.Add(("dayEntries", "Day numbers must be greater than or equal to 1."));
                    }

                    if (dayNumbers.Count != dayNumbers.Distinct().Count())
                    {
                        errors.Add(("dayEntries", "Duplicate day numbers are not allowed."));
                    }

                    if (args.Updates.DayEntries.Any(e => e.AmountPerBird <= 0))
                    {
                        errors.Add(("dayEntries", "Amount must be greater than 0."));
                    }

                    var validWeightUnits = new[] { UnitOfMeasure.Kilogram, UnitOfMeasure.Gram, UnitOfMeasure.Pound };
                    foreach (var entry in args.Updates.DayEntries)
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
            }

            return errors;
        }
    }
}

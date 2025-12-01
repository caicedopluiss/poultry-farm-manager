using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.Core.Models.BatchActivities;

namespace PoultryFarmManager.Application.Commands.Batches;

public sealed class RegisterMortalityCommand
{
    public record Args(Guid BatchId, NewMortalityRegistrationDto NewMortalityRegistration);
    public record Result(MortalityRegistrationDto MortalityRegistration);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        private Batch? batch;

        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            // Create mortality registration
            var mortalityRegistration = args.NewMortalityRegistration.Map();
            mortalityRegistration.BatchId = args.BatchId;

            var createdMortality = await unitOfWork.BatchActivities.CreateAsync(mortalityRegistration, cancellationToken);

            // Update batch population counts based on sex specification
            var numberOfDeaths = args.NewMortalityRegistration.NumberOfDeaths;

            // Parse Sex string to enum (validation already done in ValidateAsync)
            var sexEnum = Enum.Parse<Sex>(args.NewMortalityRegistration.Sex, ignoreCase: true);

            // Deaths are for a specific sex
            switch (sexEnum)
            {
                case Sex.Unsexed:
                    batch!.UnsexedCount = Math.Max(0, batch.UnsexedCount - numberOfDeaths);
                    break;
                case Sex.Male:
                    batch!.MaleCount = Math.Max(0, batch.MaleCount - numberOfDeaths);
                    break;
                case Sex.Female:
                    batch!.FemaleCount = Math.Max(0, batch.FemaleCount - numberOfDeaths);
                    break;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var mortalityDto = new MortalityRegistrationDto().Map((MortalityRegistrationBatchActivity)createdMortality);
            var result = new Result(mortalityDto);

            return result;
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (args.BatchId == Guid.Empty)
            {
                errors.Add(("batchId", "Batch ID is required."));
            }
            else
            {
                batch = await unitOfWork.Batches.GetByIdAsync(args.BatchId, cancellationToken, true);
                if (batch is null)
                {
                    throw new InvalidOperationException($"Batch with ID {args.BatchId} not found.");
                }
            }

            if (args.NewMortalityRegistration.NumberOfDeaths <= 0)
            {
                errors.Add(("numberOfDeaths", "Number of deaths must be greater than zero."));
            }

            if (!Utils.IsIso8601DateStringValid(args.NewMortalityRegistration.DateClientIsoString))
            {
                errors.Add(("dateClientIsoString", "Date is not a valid ISO 8601 date string."));
            }

            if (!string.IsNullOrWhiteSpace(args.NewMortalityRegistration.Notes) && args.NewMortalityRegistration.Notes.Length > 500)
            {
                errors.Add(("notes", "Notes cannot exceed 500 characters."));
            }

            // Validate Sex string
            if (string.IsNullOrWhiteSpace(args.NewMortalityRegistration.Sex))
            {
                errors.Add(("sex", "Sex is required."));
            }
            else if (!Enum.TryParse<Sex>(args.NewMortalityRegistration.Sex, ignoreCase: true, out var sexEnum))
            {
                errors.Add(("sex", $"Invalid sex value: '{args.NewMortalityRegistration.Sex}'. Valid values are: Unsexed, Male, Female."));
            }
            else
            {
                // Validate if the mortality reduction is possible based on available population
                if (batch is not null && args.NewMortalityRegistration.NumberOfDeaths > 0)
                {
                    var numberOfDeaths = args.NewMortalityRegistration.NumberOfDeaths;

                    switch (sexEnum)
                    {
                        case Sex.Unsexed:
                            if (batch.UnsexedCount < numberOfDeaths)
                            {
                                errors.Add(("numberOfDeaths", $"Cannot register {numberOfDeaths} unsexed deaths. Only {batch.UnsexedCount} unsexed birds available in the batch."));
                            }
                            break;
                        case Sex.Male:
                            if (batch.MaleCount < numberOfDeaths)
                            {
                                errors.Add(("numberOfDeaths", $"Cannot register {numberOfDeaths} male deaths. Only {batch.MaleCount} male birds available in the batch."));
                            }
                            break;
                        case Sex.Female:
                            if (batch.FemaleCount < numberOfDeaths)
                            {
                                errors.Add(("numberOfDeaths", $"Cannot register {numberOfDeaths} female deaths. Only {batch.FemaleCount} female birds available in the batch."));
                            }
                            break;
                    }
                }
            }

            return errors;
        }
    }
}

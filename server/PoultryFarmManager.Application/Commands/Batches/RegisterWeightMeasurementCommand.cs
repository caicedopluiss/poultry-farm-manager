using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.Core.Models.BatchActivities;

namespace PoultryFarmManager.Application.Commands.Batches;

public sealed class RegisterWeightMeasurementCommand
{
    public record Args(Guid BatchId, NewWeightMeasurementDto NewWeightMeasurement);
    public record Result(WeightMeasurementActivityDto WeightMeasurement);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        private Batch? batch;

        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            // Create weight measurement activity
            var weightMeasurement = args.NewWeightMeasurement.Map();
            weightMeasurement.BatchId = args.BatchId;

            var createdActivity = await unitOfWork.BatchActivities.CreateAsync(weightMeasurement, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var weightMeasurementDto = WeightMeasurementActivityDto.MapFrom((WeightMeasurementBatchActivity)createdActivity);
            var result = new Result(weightMeasurementDto);

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
                // Verify batch exists
                batch = await unitOfWork.Batches.GetByIdAsync(args.BatchId, track: false, cancellationToken) ?? throw new InvalidOperationException($"Batch with ID {args.BatchId} not found.");

                // Verify batch status is Active
                if (batch.Status != BatchStatus.Active)
                {
                    errors.Add(("batchId", $"Cannot register weight measurement for a batch with status '{batch.Status}'. Only Active batches can have weight measurements recorded."));
                }
            }

            if (args.NewWeightMeasurement.AverageWeight <= 0)
            {
                errors.Add(("averageWeight", "Average weight must be greater than zero."));
            }

            if (args.NewWeightMeasurement.SampleSize <= 0)
            {
                errors.Add(("sampleSize", "Sample size must be greater than zero."));
            }

            if (!Utils.IsIso8601DateStringValid(args.NewWeightMeasurement.DateClientIsoString))
            {
                errors.Add(("dateClientIsoString", "Date is not a valid ISO 8601 date string."));
            }

            if (!string.IsNullOrWhiteSpace(args.NewWeightMeasurement.Notes) && args.NewWeightMeasurement.Notes.Length > 500)
            {
                errors.Add(("notes", "Notes cannot exceed 500 characters."));
            }

            // Validate UnitOfMeasure string
            if (string.IsNullOrWhiteSpace(args.NewWeightMeasurement.UnitOfMeasure))
            {
                errors.Add(("unitOfMeasure", "Unit of measure is required."));
            }
            else if (!Enum.TryParse<UnitOfMeasure>(args.NewWeightMeasurement.UnitOfMeasure, ignoreCase: true, out var unitOfMeasure))
            {
                errors.Add(("unitOfMeasure", $"Invalid unit of measure: '{args.NewWeightMeasurement.UnitOfMeasure}'."));
            }
            else
            {
                // Only allow weight units for weight measurements
                var validWeightUnits = new[] { UnitOfMeasure.Kilogram, UnitOfMeasure.Gram, UnitOfMeasure.Pound };
                if (!validWeightUnits.Contains(unitOfMeasure))
                {
                    errors.Add(("unitOfMeasure", $"Invalid unit for weight measurement: '{args.NewWeightMeasurement.UnitOfMeasure}'. Only weight units (Kilogram, Gram, Pound) are allowed."));
                }
            }

            return errors;
        }
    }
}

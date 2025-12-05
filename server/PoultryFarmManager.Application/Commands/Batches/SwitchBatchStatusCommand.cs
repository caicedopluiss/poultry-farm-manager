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

public sealed class SwitchBatchStatusCommand
{
    public record Args(Guid BatchId, NewStatusSwitchDto StatusSwitch);
    public record Result(StatusSwitchActivityDto StatusSwitch);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        private Batch? batch;

        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            // Create status switch activity
            var statusSwitch = args.StatusSwitch.Map();
            statusSwitch.BatchId = args.BatchId;

            var createdActivity = await unitOfWork.BatchActivities.CreateAsync(statusSwitch, cancellationToken);

            // Update batch status
            batch!.Status = statusSwitch.NewStatus;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var statusSwitchDto = StatusSwitchActivityDto.MapFrom((StatusSwitchBatchActivity)createdActivity);
            var result = new Result(statusSwitchDto);

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

            if (!Utils.IsIso8601DateStringValid(args.StatusSwitch.DateClientIsoString))
            {
                errors.Add(("dateClientIsoString", "Date is not a valid ISO 8601 date string."));
            }

            if (!string.IsNullOrWhiteSpace(args.StatusSwitch.Notes) && args.StatusSwitch.Notes.Length > 500)
            {
                errors.Add(("notes", "Notes cannot exceed 500 characters."));
            }

            // Validate status transition
            if (!string.IsNullOrWhiteSpace(args.StatusSwitch.NewStatus))
            {
                if (!Enum.TryParse<BatchStatus>(args.StatusSwitch.NewStatus, ignoreCase: true, out var newStatus))
                {
                    errors.Add(("newStatus", $"Invalid status value: '{args.StatusSwitch.NewStatus}'. Valid values are: Active, Processed, ForSale, Sold, Canceled."));
                }
                else if (batch is not null)
                {
                    // Validate state transition rules
                    var currentStatus = batch.Status;
                    var isValidTransition = IsValidStatusTransition(currentStatus, newStatus);

                    if (!isValidTransition)
                    {
                        errors.Add(("newStatus", $"Invalid status transition from '{currentStatus}' to '{newStatus}'. " +
                            GetValidTransitionsMessage(currentStatus)));
                    }
                }
            }
            else
            {
                errors.Add(("newStatus", "New status is required."));
            }

            return errors;
        }

        private static bool IsValidStatusTransition(BatchStatus currentStatus, BatchStatus newStatus)
        {
            return currentStatus switch
            {
                BatchStatus.Active => newStatus is BatchStatus.Processed or BatchStatus.ForSale or BatchStatus.Canceled,
                BatchStatus.Processed => newStatus is BatchStatus.ForSale,
                BatchStatus.ForSale => newStatus is BatchStatus.Sold,
                BatchStatus.Canceled => false,
                BatchStatus.Sold => false,
                _ => false
            };
        }

        private static string GetValidTransitionsMessage(BatchStatus currentStatus)
        {
            return currentStatus switch
            {
                BatchStatus.Active => "Valid transitions are: Processed, ForSale, Canceled.",
                BatchStatus.Processed => "Valid transition is: ForSale.",
                BatchStatus.ForSale => "Valid transition is: Sold.",
                _ => "No valid transitions available from this status."
            };
        }
    }
}

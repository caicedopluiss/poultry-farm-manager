using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Application.Commands.Batches;

public sealed class UpdateBatchNotesCommand
{
    public record Args(Guid BatchId, string? Notes);

    public record Result(bool Success);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        private Batch? batch;

        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            // Convert empty/whitespace strings to null
            batch!.Notes = string.IsNullOrWhiteSpace(args.Notes) ? null : args.Notes;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new Result(true);
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
                // Verify batch exists. Tracking for potential updates
                batch = await unitOfWork.Batches.GetByIdAsync(args.BatchId, track: true, cancellationToken: cancellationToken) ??
                    throw new InvalidOperationException($"Batch with ID {args.BatchId} not found.");
            }

            // Validate trimmed notes length
            if (!string.IsNullOrWhiteSpace(args.Notes) && args.Notes.Trim().Length > 500)
            {
                errors.Add(("notes", "Notes cannot exceed 500 characters."));
            }

            return errors;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Application.Commands.Batches;

public sealed class UpdateBatchNameCommand
{
    public record Args(Guid BatchId, string Name);
    public record Result(BatchDto UpdatedBatch);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        private Batch? batch;

        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            batch!.Name = args.Name;
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var batchDto = new BatchDto().Map(batch);
            var result = new Result(batchDto);

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
                // Verify batch exists. Tracking for potential updates
                batch = await unitOfWork.Batches.GetByIdAsync(args.BatchId, track: true, cancellationToken: cancellationToken) ??
                    throw new InvalidOperationException($"Batch with ID {args.BatchId} not found.");
            }

            if (string.IsNullOrWhiteSpace(args.Name))
            {
                errors.Add(("name", "Name is required."));
            }
            else if (args.Name.Length > 100)
            {
                errors.Add(("name", "Name cannot exceed 100 characters."));
            }
            else
            {
                var existingBatch = await unitOfWork.Batches.GetByNameAsync(args.Name, cancellationToken);
                if (existingBatch != null && existingBatch.Id != args.BatchId)
                {
                    errors.Add(("name", "A batch with this name already exists."));
                }
            }

            return errors;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Application.Commands.Batches;

public sealed class AssignFeedingTableToBatchCommand
{
    public record Args(Guid BatchId, Guid? FeedingTableId);
    public record Result(BatchDto UpdatedBatch);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        private Batch? batch;

        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            batch!.FeedingTableId = args.FeedingTableId;
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new Result(new BatchDto().Map(batch));
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string, string)>();

            if (args.BatchId == Guid.Empty)
            {
                errors.Add(("batchId", "Batch ID is required."));
                return errors;
            }

            batch = await unitOfWork.Batches.GetByIdAsync(args.BatchId, track: true, cancellationToken: cancellationToken);
            if (batch == null)
            {
                errors.Add(("batchId", $"Batch with ID {args.BatchId} not found."));
                return errors;
            }

            if (args.FeedingTableId.HasValue)
            {
                var feedingTable = await unitOfWork.FeedingTables.GetByIdAsync(args.FeedingTableId.Value, ct: cancellationToken);
                if (feedingTable == null)
                {
                    errors.Add(("feedingTableId", $"Feeding table with ID {args.FeedingTableId} not found."));
                }
            }

            return errors;
        }
    }
}

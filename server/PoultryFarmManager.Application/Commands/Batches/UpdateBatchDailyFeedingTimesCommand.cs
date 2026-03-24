using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Application.Commands.Batches;

public sealed class UpdateBatchDailyFeedingTimesCommand
{
    public record Args(Guid BatchId, int? DailyFeedingTimes);
    public record Result(BatchDto UpdatedBatch);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        private Batch? batch;

        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            batch!.DailyFeedingTimes = args.DailyFeedingTimes;
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new Result(new BatchDto().Map(batch));
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (args.BatchId == Guid.Empty)
            {
                errors.Add(("batchId", "Batch ID is required."));
                return errors;
            }

            batch = await unitOfWork.Batches.GetByIdAsync(args.BatchId, track: true, cancellationToken: cancellationToken);
            if (batch is null)
            {
                errors.Add(("batchId", "Batch not found."));
                return errors;
            }

            if (args.DailyFeedingTimes.HasValue && args.DailyFeedingTimes.Value < 1)
            {
                errors.Add(("dailyFeedingTimes", "Daily feeding times must be at least 1."));
            }

            return errors;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Operations.DTOs;
using PoultryFarmManager.Core.Operations;
using SharedLib.CQRS;

namespace PoultryFarmManager.Application.Operations.Queries;

public class GetBroilerBatchByIdQuery
{
    public record Args(Guid Id, bool IncludeFinancialTransaction);
    public record Result(BroilerBatchDto? Batch);

    public class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var batch = await unitOfWork.BroilerBatches.GetByIdAsync(args.Id, includeFinancialTransaction: args.IncludeFinancialTransaction, cancellationToken: cancellationToken);
            if (batch is null) return new Result(null);

            var batchDto = BroilerBatchDto.FromCore(batch);

            // Load WeightMeasurement activities for the batch
            var activities = await unitOfWork.Activities
                .GetActivitiesAsync(batch.Id, activityType: ActivityType.WeightMeasurement, cancellationToken: cancellationToken);

            if (activities.Count != 0)
            {
                // Get the last weight activity if exists
                var lastWeightActivity = activities.OrderByDescending(a => a.Date).FirstOrDefault();
                if (lastWeightActivity is not null)
                {
                    var activityDto = ActivityDto.FromCore(lastWeightActivity);
                    batchDto.SetLastWeightActivity(activityDto);
                }
            }

            return new Result(batchDto);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (args.Id == Guid.Empty)
            {
                errors.Add(("id", "Batch ID cannot be empty."));
            }

            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }
    }
}
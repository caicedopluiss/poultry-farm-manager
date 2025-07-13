using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Operations.DTOs;
using PoultryFarmManager.Core.Operations;
using SharedLib.CQRS;

namespace PoultryFarmManager.Application.Operations.Queries;

public sealed class ReadAllBroilerBatchQuery
{
    public record Args();
    public record Result(IEnumerable<BroilerBatchDto> Batches);

    public class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var batches = await unitOfWork.BroilerBatches.GetAllAsync(true, cancellationToken);
            var batchDtos = new List<BroilerBatchDto>();
            foreach (var batch in batches)
            {
                // Convert to DTO
                var batchDto = BroilerBatchDto.FromCore(batch);

                // Load WeightMeasurement activities for each batch
                var activities = await unitOfWork.Activities
                    .GetActivitiesAsync(batch.Id,
                        activityType: ActivityType.WeightMeasurement,
                        cancellationToken: cancellationToken);

                if (activities.Count != 0)
                {
                    // Get the last activity if exists
                    var lastActivity = activities
                        .OrderByDescending(a => a.Date)
                        .FirstOrDefault();
                    if (lastActivity is not null)
                    {
                        var activityDto = ActivityDto.FromCore(lastActivity);
                        batchDto.SetLastWeightActivity(activityDto);
                    }
                }
                batchDtos.Add(batchDto);
            }

            return new Result(batchDtos);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            // No specific validation needed for reading all batches
            return Task.FromResult<IEnumerable<(string field, string error)>>([]);
        }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models.BatchActivities;

namespace PoultryFarmManager.Application.Queries.Batches;

public class GetBatchByIdQuery
{
    public record Args(Guid Id);
    public record Result(
        BatchDto? Batch,
        IEnumerable<BatchActivityDto> Activities);

    public class Handler(IBatchesRepository batchesRepository, IBatchActivitiesRepository activitiesRepository) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var batch = await batchesRepository.GetByIdAsync(args.Id, cancellationToken);
            if (batch == null)
            {
                return new Result(null, []);
            }

            var batchDto = new BatchDto().Map(batch);

            // Get all activities for this batch and map to DTOs
            var activities = await activitiesRepository.GetAllByBatchIdAsync(args.Id, cancellationToken: cancellationToken);
            var activityDtos = activities
                .Select(activity => activity switch
                {
                    MortalityRegistrationBatchActivity m => MortalityRegistrationActivityDto.MapFrom(m) as BatchActivityDto,
                    StatusSwitchBatchActivity s => StatusSwitchActivityDto.MapFrom(s),
                    ProductConsumptionBatchActivity p => ProductConsumptionActivityDto.MapFrom(p),
                    _ => throw new InvalidOperationException($"Unknown batch activity type: {activity.GetType().Name}")
                })
                .ToList();

            return new Result(batchDto, activityDtos);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var validationErrors = new List<(string field, string error)>();

            if (args.Id == Guid.Empty)
            {
                validationErrors.Add(("Id", "Id cannot be empty"));
            }

            return Task.FromResult<IEnumerable<(string field, string error)>>(validationErrors);
        }
    }
}

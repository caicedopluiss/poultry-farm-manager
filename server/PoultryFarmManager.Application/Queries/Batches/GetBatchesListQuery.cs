using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.Batches;

public class GetBatchesListQuery
{
    public record Args();
    public record Result(IEnumerable<BatchDto> Batches);

    public class Handler(IBatchesRepository batchesRepository, IBatchActivitiesRepository activitiesRepository) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var batches = await batchesRepository.GetAllAsync(cancellationToken);

            // Fetch first status switch dates for all batches in a single query
            var batchIds = batches.Select(b => b.Id);
            var firstStatusSwitchDates = await activitiesRepository.GetFirstStatusSwitchByBatchIdsAsync(batchIds, cancellationToken);

            // Map batches to DTOs with their corresponding first status change date
            var batchDtos = batches.Select(batch =>
            {
                firstStatusSwitchDates.TryGetValue(batch.Id, out var firstStatusChangeDate);
                return new BatchDto().Map(batch, firstStatusChangeDate: firstStatusChangeDate);
            });

            var result = new Result(batchDtos);
            return result;
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<(string field, string error)>());
        }
    }
}

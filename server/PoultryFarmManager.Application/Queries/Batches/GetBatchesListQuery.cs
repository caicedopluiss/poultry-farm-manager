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

    public class Handler(IBatchesRepository batchesRepository) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var batches = await batchesRepository.GetAllAsync(cancellationToken);

            var result = new Result(batches.Select(b => new BatchDto().Map(b)));
            return result;
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<(string field, string error)>());
        }
    }
}

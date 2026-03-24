using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.FeedingTables;

public sealed class GetFeedingTablesListQuery
{
    public record Args();
    public record Result(IEnumerable<FeedingTableDto> FeedingTables);

    public sealed class Handler(IFeedingTablesRepository feedingTablesRepository) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var tables = await feedingTablesRepository.GetAllAsync(cancellationToken);
            var dtos = tables.Select(t => new FeedingTableDto().Map(t));
            return new Result(dtos);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<(string, string)>());
        }
    }
}

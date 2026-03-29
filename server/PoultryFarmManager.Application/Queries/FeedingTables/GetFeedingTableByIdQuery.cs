using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.FeedingTables;

public sealed class GetFeedingTableByIdQuery
{
    public record Args(Guid FeedingTableId);
    public record Result(FeedingTableDto? FeedingTable);

    public sealed class Handler(IFeedingTablesRepository feedingTablesRepository) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var table = await feedingTablesRepository.GetByIdAsync(args.FeedingTableId, ct: cancellationToken);
            var dto = table != null ? new FeedingTableDto().Map(table) : null;
            return new Result(dto);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string, string)>();

            if (args.FeedingTableId == Guid.Empty)
            {
                errors.Add(("feedingTableId", "Feeding table ID is required."));
            }

            return Task.FromResult<IEnumerable<(string, string)>>(errors);
        }
    }
}

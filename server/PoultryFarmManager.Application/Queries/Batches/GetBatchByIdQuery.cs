using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.Batches;

public class GetBatchByIdQuery
{
    public record Args(Guid Id);
    public record Result(BatchDto? Batch);

    public class Handler(IBatchesRepository batchesRepository) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var batch = await batchesRepository.GetByIdAsync(args.Id, cancellationToken);
            var result = new Result(batch != null ? new BatchDto().Map(batch) : null);

            return result;
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

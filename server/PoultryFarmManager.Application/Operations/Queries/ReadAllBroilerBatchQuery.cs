using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Operations.DTOs;
using PoultryFarmManager.Application.Operations.Repositories;
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
            var batchDtos = batches.Select(x => BroilerBatchDto.FromCore(x));

            return new Result(batchDtos);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            // No specific validation needed for reading all batches
            return Task.FromResult<IEnumerable<(string field, string error)>>(new List<(string field, string error)>());
        }
    }

}
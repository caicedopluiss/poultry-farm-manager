using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.SaleOrders;

public class GetSaleOrdersByBatchIdQuery
{
    public record Args(Guid BatchId);
    public record Result(IEnumerable<SaleOrderDto> SaleOrders);

    public class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var saleOrders = await unitOfWork.SaleOrders.GetByBatchIdAsync(args.BatchId, cancellationToken);
            var dtos = saleOrders.Select(so => new SaleOrderDto().Map(so)).ToList();
            return new Result(dtos);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();
            if (args.BatchId == Guid.Empty)
                errors.Add(("batchId", "Batch ID cannot be empty."));
            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }
    }
}

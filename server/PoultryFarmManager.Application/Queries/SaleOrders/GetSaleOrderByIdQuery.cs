using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.SaleOrders;

public class GetSaleOrderByIdQuery
{
    public record Args(Guid Id);
    public record Result(SaleOrderDto? SaleOrder);

    public class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var saleOrder = await unitOfWork.SaleOrders.GetByIdAsync(args.Id, track: false, cancellationToken);
            if (saleOrder is null) return new Result(null);

            var dto = new SaleOrderDto().Map(saleOrder);
            return new Result(dto);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();
            if (args.Id == Guid.Empty)
                errors.Add(("id", "ID cannot be empty."));
            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }
    }
}

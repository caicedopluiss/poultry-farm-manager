using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Application.Commands.SaleOrders;

public sealed class CancelSaleOrderCommand
{
    public record Args(Guid SaleOrderId);
    public record Result(SaleOrderDto CancelledSaleOrder);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var saleOrder = await unitOfWork.SaleOrders.GetByIdAsync(args.SaleOrderId, track: true, cancellationToken);
            saleOrder!.Status = SaleOrderStatus.Cancelled;

            await unitOfWork.SaleOrders.UpdateAsync(saleOrder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var loaded = await unitOfWork.SaleOrders.GetByIdAsync(args.SaleOrderId, track: false, cancellationToken);
            var resultDto = new SaleOrderDto().Map(loaded!);
            return new Result(resultDto);
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (args.SaleOrderId == Guid.Empty)
            {
                errors.Add(("saleOrderId", "Sale order ID is required."));
                return errors;
            }

            var saleOrder = await unitOfWork.SaleOrders.GetByIdAsync(args.SaleOrderId, track: false, cancellationToken);
            if (saleOrder is null)
            {
                errors.Add(("saleOrderId", "Sale order not found."));
                return errors;
            }

            if (saleOrder.Status == SaleOrderStatus.Cancelled)
                errors.Add(("saleOrderId", "Sale order is already cancelled."));

            if (saleOrder.Status == SaleOrderStatus.Paid)
                errors.Add(("saleOrderId", "Cannot cancel a fully paid sale order."));

            return errors;
        }
    }
}

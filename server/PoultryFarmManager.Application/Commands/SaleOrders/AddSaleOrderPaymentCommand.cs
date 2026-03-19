using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Application.Commands.SaleOrders;

public sealed class AddSaleOrderPaymentCommand
{
    public record Args(Guid SaleOrderId, AddSaleOrderPaymentDto Payment);
    public record Result(SaleOrderDto UpdatedSaleOrder);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var saleOrder = await unitOfWork.SaleOrders.GetByIdAsync(args.SaleOrderId, track: true, cancellationToken);

            var paymentDate = Utils.ParseIso8601DateTimeString(args.Payment.DateClientIsoString).UtcDateTime;

            var transaction = new Transaction
            {
                Title = $"Payment for sale order #{args.SaleOrderId}",
                Date = paymentDate,
                Type = TransactionType.Income,
                UnitPrice = args.Payment.Amount,
                Quantity = null,
                TransactionAmount = args.Payment.Amount,
                BatchId = saleOrder!.BatchId,
                SaleOrderId = args.SaleOrderId,
                CustomerId = saleOrder.CustomerId,
                Notes = string.IsNullOrWhiteSpace(args.Payment.Notes) ? null : args.Payment.Notes.Trim()
            };

            await unitOfWork.Transactions.CreateAsync(transaction, cancellationToken);

            // Update sale order status
            var totalPaidAfter = saleOrder.TotalPaid + args.Payment.Amount;
            saleOrder.Status = totalPaidAfter >= saleOrder.TotalAmount
                ? SaleOrderStatus.Paid
                : SaleOrderStatus.PartiallyPaid;

            await unitOfWork.SaleOrders.UpdateAsync(saleOrder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with all navigation properties
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
                errors.Add(("saleOrderId", "Cannot add a payment to a cancelled sale order."));

            if (saleOrder.Status == SaleOrderStatus.Paid)
                errors.Add(("saleOrderId", "Sale order is already fully paid."));

            if (!Utils.IsIso8601DateStringValid(args.Payment.DateClientIsoString))
                errors.Add(("dateClientIsoString", "Date is not a valid ISO 8601 date string."));

            if (args.Payment.Amount <= 0)
                errors.Add(("amount", "Payment amount must be greater than zero."));

            if (!string.IsNullOrWhiteSpace(args.Payment.Notes) && args.Payment.Notes.Length > 500)
                errors.Add(("notes", "Notes cannot exceed 500 characters."));

            return errors;
        }
    }
}

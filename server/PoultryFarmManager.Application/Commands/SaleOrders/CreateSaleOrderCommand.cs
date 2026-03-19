using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Application.Commands.SaleOrders;

public sealed class CreateSaleOrderCommand
{
    public record Args(NewSaleOrderDto NewSaleOrder);
    public record Result(SaleOrderDto CreatedSaleOrder);

    private static readonly UnitOfMeasure[] MassUnits = [UnitOfMeasure.Kilogram, UnitOfMeasure.Gram, UnitOfMeasure.Pound];

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var dto = args.NewSaleOrder;

            var saleOrder = new SaleOrder
            {
                BatchId = dto.BatchId,
                CustomerId = dto.CustomerId,
                Date = Utils.ParseIso8601DateTimeString(dto.DateClientIsoString).UtcDateTime,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                Status = SaleOrderStatus.Pending,
                PricePerKg = dto.PricePerUnit,
                Items = dto.Items.Select(i => i.Map()).ToList()
            };

            var created = await unitOfWork.SaleOrders.CreateAsync(saleOrder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var loaded = await unitOfWork.SaleOrders.GetByIdAsync(created.Id, track: false, cancellationToken);
            var resultDto = new SaleOrderDto().Map(loaded!);
            return new Result(resultDto);
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();
            var dto = args.NewSaleOrder;

            if (dto.BatchId == Guid.Empty)
            {
                errors.Add(("batchId", "Batch ID is required."));
            }
            else
            {
                var batch = await unitOfWork.Batches.GetByIdAsync(dto.BatchId, cancellationToken: cancellationToken);
                if (batch is null)
                    errors.Add(("batchId", "Batch not found."));
            }

            if (dto.CustomerId == Guid.Empty)
            {
                errors.Add(("customerId", "Customer ID is required."));
            }
            else
            {
                var customer = await unitOfWork.Persons.GetByIdAsync(dto.CustomerId, cancellationToken: cancellationToken);
                if (customer is null)
                    errors.Add(("customerId", "Customer not found."));
            }

            if (!Utils.IsIso8601DateStringValid(dto.DateClientIsoString))
                errors.Add(("dateClientIsoString", "Date is not a valid ISO 8601 date string."));

            if (dto.PricePerUnit <= 0)
                errors.Add(("pricePerUnit", "Price per unit must be greater than zero."));

            if (!string.IsNullOrWhiteSpace(dto.Notes) && dto.Notes.Length > 500)
                errors.Add(("notes", "Notes cannot exceed 500 characters."));

            var itemsList = dto.Items?.ToList() ?? [];
            if (itemsList.Count == 0)
            {
                errors.Add(("items", "At least one item is required."));
            }
            else
            {
                for (var i = 0; i < itemsList.Count; i++)
                {
                    var item = itemsList[i];

                    if (item.Weight <= 0)
                        errors.Add(($"items[{i}].weight", "Weight must be greater than zero."));

                    if (string.IsNullOrWhiteSpace(item.UnitOfMeasure))
                    {
                        errors.Add(($"items[{i}].unitOfMeasure", "Unit of measure is required."));
                    }
                    else if (!Enum.TryParse<UnitOfMeasure>(item.UnitOfMeasure, ignoreCase: true, out var itemUom))
                    {
                        errors.Add(($"items[{i}].unitOfMeasure", $"Invalid unit of measure: '{item.UnitOfMeasure}'."));
                    }
                    else if (!MassUnits.Contains(itemUom))
                    {
                        errors.Add(($"items[{i}].unitOfMeasure", "Unit of measure must be a mass unit (Kilogram, Gram or Pound)."));
                    }

                    if (!Utils.IsIso8601DateStringValid(item.ProcessedDateClientIsoString))
                        errors.Add(($"items[{i}].processedDateClientIsoString", "Processed date is not a valid ISO 8601 date string."));
                }
            }

            return errors;
        }
    }
}

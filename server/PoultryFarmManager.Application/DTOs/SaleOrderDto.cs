using System;
using System.Collections.Generic;
using System.Linq;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Application.DTOs;

public record NewSaleOrderItemDto(
    decimal Weight,
    string UnitOfMeasure,
    string ProcessedDateClientIsoString)
{
    public SaleOrderItem Map(SaleOrderItem? to = null)
    {
        var result = to ?? new();

        result.Weight = Weight;
        result.UnitOfMeasure = Enum.Parse<UnitOfMeasure>(UnitOfMeasure, ignoreCase: true);
        result.ProcessedDate = Utils.ParseIso8601DateTimeString(ProcessedDateClientIsoString).UtcDateTime;

        return result;
    }
}

public record NewSaleOrderDto(
    Guid BatchId,
    Guid CustomerId,
    string DateClientIsoString,
    decimal PricePerUnit,
    IEnumerable<NewSaleOrderItemDto> Items,
    string? Notes);

public record AddSaleOrderPaymentDto(
    string DateClientIsoString,
    decimal Amount,
    string? Notes);

public record SaleOrderItemDto(
    Guid Id,
    decimal Weight,
    string UnitOfMeasure,
    string ProcessedDate)
{
    public SaleOrderItemDto() : this(Guid.Empty, 0m, string.Empty, string.Empty) { }

    public SaleOrderItemDto Map(SaleOrderItem from)
    {
        return this with
        {
            Id = from.Id,
            Weight = from.Weight,
            UnitOfMeasure = from.UnitOfMeasure.ToString(),
            ProcessedDate = from.ProcessedDate.ToString(Constants.DateTimeFormat)
        };
    }
}

public record SaleOrderPaymentDto(
    Guid TransactionId,
    string Date,
    decimal Amount,
    string? Notes)
{
    public SaleOrderPaymentDto() : this(Guid.Empty, string.Empty, 0m, null) { }

    public SaleOrderPaymentDto Map(Core.Models.Finance.Transaction from)
    {
        return this with
        {
            TransactionId = from.Id,
            Date = from.Date.ToString(Constants.DateTimeFormat),
            Amount = from.TransactionAmount,
            Notes = from.Notes
        };
    }
}

public record SaleOrderDto(
    Guid Id,
    Guid BatchId,
    string? BatchName,
    Guid CustomerId,
    string CustomerFullName,
    string Date,
    string Status,
    string? Notes,
    decimal PricePerUnit,
    IEnumerable<SaleOrderItemDto> Items,
    IEnumerable<SaleOrderPaymentDto> Payments,
    decimal TotalWeight,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal PendingAmount)
{
    public SaleOrderDto() : this(
        Guid.Empty, Guid.Empty, null, Guid.Empty, string.Empty,
        string.Empty, string.Empty, null, 0m, [], [], 0m, 0m, 0m, 0m)
    { }

    public SaleOrderDto Map(SaleOrder from)
    {
        var items = from.Items.Select(i => new SaleOrderItemDto().Map(i)).ToList();
        var payments = from.Payments.Select(p => new SaleOrderPaymentDto().Map(p)).ToList();

        return this with
        {
            Id = from.Id,
            BatchId = from.BatchId,
            BatchName = from.Batch?.Name,
            CustomerId = from.CustomerId,
            CustomerFullName = from.Customer is not null
                ? $"{from.Customer.FirstName} {from.Customer.LastName}"
                : string.Empty,
            Date = from.Date.ToString(Constants.DateTimeFormat),
            Status = from.Status.ToString(),
            Notes = from.Notes,
            PricePerUnit = from.PricePerKg,
            Items = items,
            Payments = payments,
            TotalWeight = from.Items.Sum(i => i.Weight),
            TotalAmount = from.TotalAmount,
            TotalPaid = from.TotalPaid,
            PendingAmount = from.PendingAmount
        };
    }
}

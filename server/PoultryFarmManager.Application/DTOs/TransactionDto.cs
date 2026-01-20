using System;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Application.DTOs;

public record NewTransactionDto(
    string Title,
    string DateClientIsoString,
    string Type,
    decimal UnitPrice,
    int? Quantity,
    decimal TransactionAmount,
    string? Notes,
    Guid? ProductVariantId,
    Guid? BatchId,
    Guid? VendorId,
    Guid? CustomerId)
{
    public Transaction Map(Transaction? to = null)
    {
        var result = to ?? new();

        result.Title = Title;
        result.Date = Utils.ParseIso8601DateTimeString(DateClientIsoString).UtcDateTime;
        result.Type = Enum.Parse<TransactionType>(Type, ignoreCase: true);
        result.UnitPrice = UnitPrice;
        result.Quantity = Quantity;
        result.TransactionAmount = TransactionAmount;
        result.Notes = Notes;
        result.ProductVariantId = ProductVariantId;
        result.BatchId = BatchId;
        result.VendorId = VendorId;
        result.CustomerId = CustomerId;

        return result;
    }
}

public record TransactionDto(
    Guid Id,
    string Title,
    string Date,
    string Type,
    decimal UnitPrice,
    int? Quantity,
    decimal TransactionAmount,
    decimal TotalAmount,
    string? Notes,
    Guid? ProductVariantId,
    string? ProductVariantName,
    Guid? BatchId,
    string? BatchName,
    Guid? VendorId,
    string? VendorName,
    Guid? CustomerId,
    string? CustomerName
)
{
    /// <summary>
    /// Parameterless constructor for mapping dto instance from core model/entity.
    /// </summary>
    public TransactionDto() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        0m,
        null,
        0m,
        0m,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null)
    {
    }

    public TransactionDto Map(Transaction from, TransactionDto? to = null)
    {
        return to is not null ? to with
        {
            Id = from.Id,
            Title = from.Title,
            Date = from.Date.ToString(Constants.DateTimeFormat),
            Type = from.Type.ToString(),
            UnitPrice = from.UnitPrice,
            Quantity = from.Quantity,
            TransactionAmount = from.TransactionAmount,
            TotalAmount = from.TotalAmount,
            Notes = from.Notes,
            ProductVariantId = from.ProductVariantId,
            ProductVariantName = from.ProductVariant?.Name,
            BatchId = from.BatchId,
            BatchName = from.Batch?.Name,
            VendorId = from.VendorId,
            VendorName = from.Vendor?.Name,
            CustomerId = from.CustomerId,
            CustomerName = from.Customer != null ? $"{from.Customer.FirstName} {from.Customer.LastName}" : null
        } : new TransactionDto(
            from.Id,
            from.Title,
            from.Date.ToString(Constants.DateTimeFormat),
            from.Type.ToString(),
            from.UnitPrice,
            from.Quantity,
            from.TransactionAmount,
            from.TotalAmount,
            from.Notes,
            from.ProductVariantId,
            from.ProductVariant?.Name,
            from.BatchId,
            from.Batch?.Name,
            from.VendorId,
            from.Vendor?.Name,
            from.CustomerId,
            from.Customer != null ? $"{from.Customer.FirstName} {from.Customer.LastName}" : null
        );
    }
}

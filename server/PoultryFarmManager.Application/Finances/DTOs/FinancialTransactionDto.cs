using System;
using PoultryFarmManager.Core.Finances;
using PoultryFarmManager.Core.Finances.Models;

namespace PoultryFarmManager.Application.Finances.DTOs;

public record NewFinancialTransactionDto
{
    public string TransactionClientDateDate { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = nameof(PaymentStatus.Pending);
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? DueClientDate { get; set; }
    public Guid? FinancialEntityId { get; set; }
    public NewFinancialEntityDto? FinancialEntityInfo { get; set; }
    public string Notes { get; set; } = string.Empty;

    public virtual FinancialTransaction ToCoreModel() => new()
    {
        TransactionDate = Utils.ParseIso8601DateTimeString(TransactionClientDateDate).UtcDateTime,
        Type = Enum.Parse<FinancialTransactionType>(Type),
        Status = Enum.Parse<PaymentStatus>(Status),
        Category = Enum.Parse<FinancialTransactionCategory>(Category),
        Amount = Amount,
        PaidAmount = PaidAmount,
        DueDate = string.IsNullOrEmpty(DueClientDate?.Trim())
            ? null
            : Utils.ParseIso8601DateTimeString(DueClientDate!).UtcDateTime,
        FinancialEntity = FinancialEntityInfo?.ToCoreModel(),
        FinancialEntityId = FinancialEntityId ?? Guid.Empty,
        Notes = Notes,
    };
};

public record FinancialTransactionDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public string TransactionClientDateDate { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = nameof(PaymentStatus.Pending);
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? DueClientDate { get; set; }
    public FinancialEntityDto? FinancialEntity { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    public static FinancialTransactionDto FromCore(FinancialTransaction transaction) => new()
    {
        Id = transaction.Id,
        TransactionClientDateDate = transaction.TransactionDate.ToString(Constants.DateTimeFormat),
        Type = transaction.Type.ToString(),
        Status = transaction.Status.ToString(),
        Category = transaction.Category.ToString(),
        Amount = transaction.Amount,
        PaidAmount = transaction.PaidAmount,
        DueClientDate = transaction.DueDate?.ToString(Constants.DateTimeFormat),
        FinancialEntity = transaction.FinancialEntity != null
            ? FinancialEntityDto.FromCore(transaction.FinancialEntity)
            : null,
        Notes = transaction.Notes,
        CreatedAt = transaction.CreatedAt,
        ModifiedAt = transaction.ModifiedAt
    };
}
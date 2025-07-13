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
        Category = Enum.Parse<FinancialTransactionCategory>(Category),
        Amount = Utils.TruncateToTwoDecimals(Amount),
        PaidAmount = PaidAmount is not null
            ? Utils.TruncateToTwoDecimals((decimal)PaidAmount)
            : null,
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
    public string TransactionDate { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = nameof(PaymentStatus.Pending);
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? DueDate { get; set; }
    public FinancialEntityDto? FinancialEntity { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    public static FinancialTransactionDto FromCore(FinancialTransaction transaction) => new()
    {
        Id = transaction.Id,
        TransactionDate = transaction.TransactionDate.ToString(Constants.DateTimeFormat),
        Type = transaction.Type.ToString(),
        Status = transaction.Status.ToString(),
        Category = transaction.Category.ToString(),
        Amount = transaction.Amount,
        PaidAmount = transaction.PaidAmount,
        DueDate = transaction.DueDate?.ToString(Constants.DateTimeFormat),
        FinancialEntity = transaction.FinancialEntity != null
            ? FinancialEntityDto.FromCore(transaction.FinancialEntity)
            : null,
        Notes = transaction.Notes,
        CreatedAt = transaction.CreatedAt,
        ModifiedAt = transaction.ModifiedAt
    };
}
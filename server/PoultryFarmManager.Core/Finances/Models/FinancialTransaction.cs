namespace PoultryFarmManager.Core.Finances.Models;

public class FinancialTransaction : DbEntityBase
{
    public DateTime TransactionDate { get; set; }
    public FinancialTransactionType Type { get; set; }
    public PaymentStatus Status { get; set; }
    public FinancialTransactionCategory Category { get; set; }
    public decimal Amount { get; set; }
    public decimal? PaidAmount { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid FinancialEntityId { get; set; }
    public string Notes { get; set; } = string.Empty;


    public FinancialEntity? FinancialEntity { get; set; } = null!;
}
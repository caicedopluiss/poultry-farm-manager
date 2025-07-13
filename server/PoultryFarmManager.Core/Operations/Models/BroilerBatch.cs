using PoultryFarmManager.Core.Finances.Models;

namespace PoultryFarmManager.Core.Operations.Models;

public class BroilerBatch : DbEntityBase
{
    public string BatchName { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public int InitialPopulation { get; set; }
    public int CurrentPopulation { get; set; }
    public string? Breed { get; set; }
    public BroilerBatchStatus Status { get; set; } = BroilerBatchStatus.Draft;
    public DateTime? ProcessingStartDate { get; set; }
    public DateTime? ProcessingEndDate { get; set; }
    public string? Notes { get; set; } = string.Empty;
    public Guid FinancialTransactionId { get; set; }

    public FinancialTransaction? FinancialTransaction { get; set; }
}


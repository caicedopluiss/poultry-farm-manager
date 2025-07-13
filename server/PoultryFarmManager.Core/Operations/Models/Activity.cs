namespace PoultryFarmManager.Core.Operations.Models;

public class Activity : DbEntityBase
{
    public Guid BroilerBatchId { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public decimal? Value { get; set; }
    public string? Unit { get; set; }
    public ActivityType Type { get; set; }

    public BroilerBatch? BroilerBatch { get; set; }
}
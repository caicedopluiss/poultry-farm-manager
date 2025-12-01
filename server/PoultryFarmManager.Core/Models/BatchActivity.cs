using System;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Core.Models;

public abstract class BatchActivity : DbEntity
{
    public Guid BatchId { get; set; }
    public BatchActivityType Type { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }

    public Batch? Batch { get; set; } = null;
}

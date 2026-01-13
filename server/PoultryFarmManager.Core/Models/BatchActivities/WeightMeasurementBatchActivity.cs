using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Core.Models.BatchActivities;

public class WeightMeasurementBatchActivity : BatchActivity
{
    public decimal AverageWeight { get; set; }
    public int SampleSize { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; }
}

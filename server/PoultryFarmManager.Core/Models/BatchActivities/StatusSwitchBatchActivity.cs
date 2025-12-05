using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Core.Models.BatchActivities;

public class StatusSwitchBatchActivity : BatchActivity
{
    public BatchStatus NewStatus { get; set; }
}

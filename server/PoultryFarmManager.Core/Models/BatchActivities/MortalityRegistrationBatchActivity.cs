using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Core.Models.BatchActivities;

public class MortalityRegistrationBatchActivity : BatchActivity
{
    public int NumberOfDeaths { get; set; }
    public Sex Sex { get; set; }
}

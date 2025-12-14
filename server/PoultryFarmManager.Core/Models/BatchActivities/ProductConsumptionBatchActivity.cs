using System;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Core.Models.BatchActivities;

public class ProductConsumptionBatchActivity : BatchActivity
{
    public Guid ProductId { get; set; }
    public decimal Stock { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; }

    public Product? Product { get; set; }
}

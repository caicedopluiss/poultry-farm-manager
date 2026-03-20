using System;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Core.Models;

public class SaleOrderItem : DbEntity
{
    public Guid SaleOrderId { get; set; }
    public SaleOrder? SaleOrder { get; set; }

    public decimal Weight { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; } = UnitOfMeasure.Kilogram;
    public DateTime ProcessedDate { get; set; }
}

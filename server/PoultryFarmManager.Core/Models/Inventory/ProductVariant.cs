using System;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Core.Models.Inventory;

public class ProductVariant : DbEntity
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public UnitOfMeasure UnitOfMeasure { get; set; }
    public decimal Stock { get; set; }
    public int Quantity { get; set; }
    public string? Description { get; set; }

    public Product? Product { get; set; }
}

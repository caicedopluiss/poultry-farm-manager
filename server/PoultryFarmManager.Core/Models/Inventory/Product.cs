using System.Collections;
using System.Collections.Generic;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Core.Models.Inventory;

public class Product : DbEntity
{
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public decimal Stock { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; }
    public string? Description { get; set; }

    public ICollection<ProductVariant>? Variants { get; set; }
}

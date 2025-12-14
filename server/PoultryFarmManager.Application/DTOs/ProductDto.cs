using System;
using System.Collections.Generic;
using System.Linq;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Application.DTOs;

public record NewProductDto(
    string Name,
    string Manufacturer,
    decimal Stock,
    string UnitOfMeasure,
    string? Description)
{
    public Product Map(Product? to = null)
    {
        var result = to ?? new();

        result.Name = Name;
        result.Manufacturer = Manufacturer;
        result.Stock = Stock;
        result.UnitOfMeasure = Enum.Parse<UnitOfMeasure>(UnitOfMeasure, ignoreCase: true);
        result.Description = Description;

        return result;
    }
}

public record ProductDto(
    Guid Id,
    string Name,
    string Manufacturer,
    decimal Stock,
    string UnitOfMeasure,
    string? Description,
    ICollection<ProductVariantDto>? Variants
)
{
    /// <summary>
    /// Parameterless constructor for mapping dto instance from core model/entity.
    /// </summary>
    public ProductDto() : this(Guid.Empty, string.Empty, string.Empty, 0, string.Empty, null, null)
    {
    }

    public ProductDto Map(Product from)
    {
        return this with
        {
            Id = from.Id,
            Name = from.Name,
            Manufacturer = from.Manufacturer,
            UnitOfMeasure = from.UnitOfMeasure.ToString(),
            Stock = from.Stock,
            Description = from.Description,
            Variants = from.Variants?.Select(v => new ProductVariantDto().Map(v)).ToList()
        };
    }
}

public record UpdateProductDto(
    string? Name,
    string? Manufacturer,
    decimal? Stock,
    string? UnitOfMeasure,
    string? Description)
{
    public void ApplyTo(Product product)
    {
        if (!string.IsNullOrWhiteSpace(Name))
        {
            product.Name = Name;
        }

        if (!string.IsNullOrWhiteSpace(Manufacturer))
        {
            product.Manufacturer = Manufacturer;
        }

        if (Stock.HasValue)
        {
            product.Stock = Stock.Value;
        }

        if (!string.IsNullOrWhiteSpace(UnitOfMeasure) && Enum.TryParse<UnitOfMeasure>(UnitOfMeasure, ignoreCase: true, out var unit))
        {
            product.UnitOfMeasure = unit;
        }

        if (!string.IsNullOrWhiteSpace(Description))
        {
            product.Description = Description;
        }
    }
}

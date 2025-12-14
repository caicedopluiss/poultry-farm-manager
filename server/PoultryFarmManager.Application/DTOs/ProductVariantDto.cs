using System;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Application.DTOs;

public record NewProductVariantDto(
    Guid ProductId,
    string Name,
    string UnitOfMeasure,
    decimal Stock,
    int Quantity,
    string? Description)
{
    public ProductVariant Map(ProductVariant? to = null)
    {
        var result = to ?? new();

        result.ProductId = ProductId;
        result.Name = Name;
        result.Stock = Stock;
        result.Quantity = Quantity;
        result.Description = Description;
        result.UnitOfMeasure = Enum.Parse<UnitOfMeasure>(UnitOfMeasure, ignoreCase: true);

        return result;
    }
}

public record ProductVariantDto(
    Guid Id,
    Guid ProductId,
    string Name,
    string UnitOfMeasure,
    decimal Stock,
    int Quantity,
    string? Description,
    ProductDto? Product
)
{
    /// <summary>
    /// Parameterless constructor for mapping dto instance from core model/entity.
    /// </summary>
    public ProductVariantDto() : this(Guid.Empty, Guid.Empty, string.Empty, string.Empty, 0, 0, null, null)
    {
    }

    public ProductVariantDto Map(ProductVariant from)
    {
        return this with
        {
            Id = from.Id,
            ProductId = from.ProductId,
            Name = from.Name,
            UnitOfMeasure = from.UnitOfMeasure.ToString(),
            Stock = from.Stock,
            Quantity = from.Quantity,
            Description = from.Description,
            Product = from.Product != null ? new ProductDto
            {
                Id = from.Product.Id,
                Name = from.Product.Name,
                Manufacturer = from.Product.Manufacturer,
                UnitOfMeasure = from.Product.UnitOfMeasure.ToString(),
                Stock = from.Product.Stock,
                Description = from.Product.Description,
                Variants = null // Avoid circular reference
            } : null
        };
    }
}

public record UpdateProductVariantDto(
    string? Name,
    string? UnitOfMeasure,
    decimal? Stock,
    int? Quantity,
    string? Description)
{
    public void ApplyTo(ProductVariant productVariant)
    {
        if (!string.IsNullOrWhiteSpace(Name))
        {
            productVariant.Name = Name;
        }

        if (!string.IsNullOrWhiteSpace(UnitOfMeasure) && Enum.TryParse<UnitOfMeasure>(UnitOfMeasure, ignoreCase: true, out var unit))
        {
            productVariant.UnitOfMeasure = unit;
        }

        if (Stock.HasValue)
        {
            productVariant.Stock = Stock.Value;
        }

        if (Quantity.HasValue)
        {
            productVariant.Quantity = Quantity.Value;
        }

        if (!string.IsNullOrWhiteSpace(Description))
        {
            productVariant.Description = Description;
        }
    }
}

public record AdjustProductVariantStockDto(
    decimal StockChange)
{
    public void ApplyTo(ProductVariant productVariant)
    {
        productVariant.Stock += StockChange;
    }
}

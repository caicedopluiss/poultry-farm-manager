using System;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Application.DTOs;

public record NewAssetStateDto(
    Guid AssetId,
    string Status,
    int Quantity,
    string? Location)
{
    public AssetState Map(AssetState? to = null)
    {
        var result = to ?? new();

        result.AssetId = AssetId;
        result.Status = Enum.Parse<AssetStatus>(Status, ignoreCase: true);
        result.Quantity = Quantity;
        result.Location = Location;

        return result;
    }
}

public record AssetStateDto(
    Guid Id,
    string Status,
    int Quantity,
    string? Location
)
{
    /// <summary>
    /// Parameterless constructor for mapping dto instance from core model/entity.
    /// </summary>
    public AssetStateDto() : this(Guid.Empty, string.Empty, 0, null)
    {
    }

    public AssetStateDto Map(AssetState from)
    {
        return this with
        {
            Id = from.Id,
            Status = from.Status.ToString(),
            Quantity = from.Quantity,
            Location = from.Location
        };
    }
}

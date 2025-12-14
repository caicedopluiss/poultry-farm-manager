using System;
using System.Collections.Generic;
using System.Linq;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Application.DTOs;

public record NewAssetDto(
    string Name,
    string? Description,
    int InitialQuantity,
    string? Notes)
{
    public NewAssetDto() : this(string.Empty, null, 1, null)
    {
    }

    public Asset Map(Asset? to = null)
    {
        var result = to ?? new();

        result.Name = Name;
        result.Description = Description;
        result.Notes = Notes;

        return result;
    }
}

public record AssetDto(
    Guid Id,
    string Name,
    string? Description,
    string? Notes,
    ICollection<AssetStateDto>? States
)
{
    /// <summary>
    /// Parameterless constructor for mapping dto instance from core model/entity.
    /// </summary>
    public AssetDto() : this(Guid.Empty, string.Empty, null, null, null)
    {
    }

    public AssetDto Map(Asset from)
    {
        return this with
        {
            Id = from.Id,
            Name = from.Name,
            Description = from.Description,
            Notes = from.Notes,
            States = from.States?.Select(s => new AssetStateDto().Map(s)).ToList()
        };
    }
}

public record UpdateAssetDto(
    string? Name,
    string? Description,
    string? Notes,
    List<AssetStateDto>? States)
{
    public void ApplyTo(Asset asset)
    {
        if (Name != null)
        {
            asset.Name = Name;
        }

        if (Description != null)
        {
            asset.Description = Description;
        }

        if (Notes != null)
        {
            asset.Notes = Notes;
        }

        if (States != null && States.Count > 0)
        {
            // Clear existing states and add new ones
            asset.States?.Clear();
            asset.States = States.Select(s =>
            {
                var state = new AssetState
                {
                    AssetId = asset.Id,
                    Status = Enum.Parse<AssetStatus>(s.Status, ignoreCase: true),
                    Quantity = s.Quantity,
                    Location = s.Location
                };

                // Only set Id if it's a valid Guid (existing state)
                if (s.Id != Guid.Empty)
                {
                    state.Id = s.Id;
                }

                return state;
            }).ToList();
        }
    }
}

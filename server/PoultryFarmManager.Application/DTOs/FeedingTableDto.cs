using System;
using System.Collections.Generic;
using System.Linq;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Application.DTOs;

public record NewFeedingTableDayEntryDto(
    int DayNumber,
    string FoodType,
    decimal AmountPerBird,
    string UnitOfMeasure,
    decimal? ExpectedBirdWeight,
    string? ExpectedBirdWeightUnitOfMeasure)
{
    public FeedingTableDayEntry Map() => new()
    {
        DayNumber = DayNumber,
        FoodType = Enum.Parse<FoodType>(FoodType, ignoreCase: true),
        AmountPerBird = AmountPerBird,
        UnitOfMeasure = Enum.Parse<UnitOfMeasure>(UnitOfMeasure, ignoreCase: true),
        ExpectedBirdWeight = ExpectedBirdWeight,
        ExpectedBirdWeightUnitOfMeasure = !string.IsNullOrWhiteSpace(ExpectedBirdWeightUnitOfMeasure)
            ? Enum.Parse<UnitOfMeasure>(ExpectedBirdWeightUnitOfMeasure, ignoreCase: true)
            : null
    };
}

public record FeedingTableDayEntryDto(
    Guid Id,
    int DayNumber,
    string FoodType,
    decimal AmountPerBird,
    string UnitOfMeasure,
    decimal? ExpectedBirdWeight,
    string? ExpectedBirdWeightUnitOfMeasure)
{
    public FeedingTableDayEntryDto() : this(Guid.Empty, 0, string.Empty, 0, string.Empty, null, null)
    {
    }

    public FeedingTableDayEntryDto Map(FeedingTableDayEntry from)
    {
        return this with
        {
            Id = from.Id,
            DayNumber = from.DayNumber,
            FoodType = from.FoodType.ToString(),
            AmountPerBird = from.AmountPerBird,
            UnitOfMeasure = from.UnitOfMeasure.ToString(),
            ExpectedBirdWeight = from.ExpectedBirdWeight,
            ExpectedBirdWeightUnitOfMeasure = from.ExpectedBirdWeightUnitOfMeasure?.ToString()
        };
    }
}

public record NewFeedingTableDto(
    string Name,
    string? Description,
    List<NewFeedingTableDayEntryDto> DayEntries)
{
    public NewFeedingTableDto() : this(string.Empty, null, [])
    {
    }

    public FeedingTable Map()
    {
        var table = new FeedingTable
        {
            Name = Name.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
        };

        foreach (var entry in DayEntries)
        {
            table.DayEntries.Add(entry.Map());
        }

        return table;
    }
}

public record FeedingTableDto(
    Guid Id,
    string Name,
    string? Description,
    ICollection<FeedingTableDayEntryDto> DayEntries)
{
    public FeedingTableDto() : this(Guid.Empty, string.Empty, null, [])
    {
    }

    public FeedingTableDto Map(FeedingTable from)
    {
        return this with
        {
            Id = from.Id,
            Name = from.Name,
            Description = from.Description,
            DayEntries = from.DayEntries
                .OrderBy(e => e.DayNumber)
                .Select(e => new FeedingTableDayEntryDto().Map(e))
                .ToList()
        };
    }
}

public record UpdateFeedingTableDto(
    string? Name,
    string? Description,
    List<NewFeedingTableDayEntryDto>? DayEntries);



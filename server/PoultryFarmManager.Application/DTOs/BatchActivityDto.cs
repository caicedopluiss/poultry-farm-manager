using System;
using System.Text.Json.Serialization;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.Core.Models.BatchActivities;

namespace PoultryFarmManager.Application.DTOs;

// Input DTOs for creating new activities
public record NewMortalityRegistrationDto(
    int NumberOfDeaths,
    string DateClientIsoString,
    string Sex,
    string? Notes)
{
    public MortalityRegistrationBatchActivity Map(MortalityRegistrationBatchActivity? to = null)
    {
        var result = to ?? new();

        result.NumberOfDeaths = NumberOfDeaths;
        result.Date = Utils.ParseIso8601DateTimeString(DateClientIsoString).UtcDateTime;
        result.Sex = Enum.Parse<Sex>(Sex, ignoreCase: true);
        result.Notes = Notes;
        result.Type = BatchActivityType.MortalityRecording;

        return result;
    }
}

public record NewStatusSwitchDto(
    string NewStatus,
    string DateClientIsoString,
    string? Notes)
{
    public StatusSwitchBatchActivity Map(StatusSwitchBatchActivity? to = null)
    {
        var result = to ?? new();

        result.NewStatus = Enum.Parse<BatchStatus>(NewStatus, ignoreCase: true);
        result.Date = Utils.ParseIso8601DateTimeString(DateClientIsoString).UtcDateTime;
        result.Notes = Notes;
        result.Type = BatchActivityType.StatusSwitch;

        return result;
    }
}

public record NewProductConsumptionDto(
    Guid ProductId,
    decimal Stock,
    string UnitOfMeasure,
    string DateClientIsoString,
    string? Notes)
{
    public ProductConsumptionBatchActivity Map(ProductConsumptionBatchActivity? to = null)
    {
        var result = to ?? new();

        result.ProductId = ProductId;
        result.Stock = Stock;
        result.UnitOfMeasure = Enum.Parse<Core.Enums.UnitOfMeasure>(UnitOfMeasure, ignoreCase: true);
        result.Date = Utils.ParseIso8601DateTimeString(DateClientIsoString).UtcDateTime;
        result.Notes = Notes;
        result.Type = BatchActivityType.ProductConsumption;

        return result;
    }
}

// Output DTOs for batch activities
// NOTE: Using JsonPolymorphic for proper derived type serialization.
// The Type property must be defined first and used as the discriminator.
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(MortalityRegistrationActivityDto), "MortalityRecording")]
[JsonDerivedType(typeof(StatusSwitchActivityDto), "StatusSwitch")]
[JsonDerivedType(typeof(ProductConsumptionActivityDto), "ProductConsumption")]
public record BatchActivityDto
{
    // IMPORTANT: Type must be the first property.
    // When using [JsonPolymorphic], System.Text.Json requires the discriminator property to be
    // serialized first in the JSON so the deserializer can identify which derived type to instantiate.
    // Property order in C# records determines JSON serialization order.
    public string Type { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public record MortalityRegistrationActivityDto : BatchActivityDto
{
    public int NumberOfDeaths { get; set; }
    public string Sex { get; set; } = string.Empty;

    public static MortalityRegistrationActivityDto MapFrom(MortalityRegistrationBatchActivity activity)
    {
        return new MortalityRegistrationActivityDto
        {
            Id = activity.Id,
            BatchId = activity.BatchId,
            Type = activity.Type.ToString(),
            Date = activity.Date.ToString(Constants.DateTimeFormat),
            Notes = activity.Notes,
            NumberOfDeaths = activity.NumberOfDeaths,
            Sex = activity.Sex.ToString()
        };
    }
}

public record StatusSwitchActivityDto : BatchActivityDto
{
    public string NewStatus { get; set; } = string.Empty;

    public static StatusSwitchActivityDto MapFrom(StatusSwitchBatchActivity activity)
    {
        return new StatusSwitchActivityDto
        {
            Id = activity.Id,
            BatchId = activity.BatchId,
            Type = activity.Type.ToString(),
            Date = activity.Date.ToString(Constants.DateTimeFormat),
            Notes = activity.Notes,
            NewStatus = activity.NewStatus.ToString()
        };
    }
}

public record ProductConsumptionActivityDto : BatchActivityDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Stock { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;

    public static ProductConsumptionActivityDto MapFrom(ProductConsumptionBatchActivity activity)
    {
        return new ProductConsumptionActivityDto
        {
            Id = activity.Id,
            BatchId = activity.BatchId,
            Type = activity.Type.ToString(),
            Date = activity.Date.ToString(Constants.DateTimeFormat),
            Notes = activity.Notes,
            ProductId = activity.ProductId,
            ProductName = activity.Product?.Name ?? string.Empty,
            Stock = activity.Stock,
            UnitOfMeasure = activity.UnitOfMeasure.ToString()
        };
    }
}

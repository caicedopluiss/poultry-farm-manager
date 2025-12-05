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

// Output DTOs for batch activities
// NOTE: Using JsonPolymorphic for proper derived type serialization.
// The Type property must be defined first and used as the discriminator.
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(MortalityRegistrationActivityDto), "MortalityRecording")]
[JsonDerivedType(typeof(StatusSwitchActivityDto), "StatusSwitch")]
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

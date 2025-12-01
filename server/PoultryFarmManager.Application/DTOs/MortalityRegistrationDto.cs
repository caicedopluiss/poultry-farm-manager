using System;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.BatchActivities;

namespace PoultryFarmManager.Application.DTOs;

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

public record MortalityRegistrationDto(
    Guid Id,
    Guid BatchId,
    string Type,
    int NumberOfDeaths,
    string Date,
    string Sex,
    string? Notes
)
{
    /// <summary>
    /// Parameterless constructor for mapping dto instance from core model/entity.
    /// </summary>
    public MortalityRegistrationDto() : this(Guid.Empty, Guid.Empty, string.Empty, 0, string.Empty, string.Empty, null)
    {

    }

    public MortalityRegistrationDto Map(MortalityRegistrationBatchActivity from, MortalityRegistrationDto? to = null)
    {
        return to is not null ? to with
        {
            Id = from.Id,
            BatchId = from.BatchId,
            Type = from.Type.ToString(),
            NumberOfDeaths = from.NumberOfDeaths,
            Date = from.Date.ToString(Constants.DateTimeFormat),
            Sex = from.Sex.ToString(),
            Notes = from.Notes
        } : new MortalityRegistrationDto(
            from.Id,
            from.BatchId,
            from.Type.ToString(),
            from.NumberOfDeaths,
            from.Date.ToString(Constants.DateTimeFormat),
            from.Sex.ToString(),
            from.Notes
        );
    }
}

using System;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Application.DTOs;

public record NewBatchDto(
    string Name,
    string StartClientDateIsoString,
    int MaleCount,
    int FemaleCount,
    int UnsexedCount,
    string? Breed,
    string? Shed)
{
    public Batch Map(Batch? to = null)
    {
        var result = to ?? new();

        result.Name = Name;
        result.StartDate = Utils.ParseIso8601DateTimeString(StartClientDateIsoString).UtcDateTime;
        result.MaleCount = MaleCount;
        result.FemaleCount = FemaleCount;
        result.UnsexedCount = UnsexedCount;
        result.Breed = Breed;
        result.Shed = Shed;

        return result;
    }
}

public record BatchDto(
    Guid Id,
    string Name,
    string? Breed,
    string Status,
    string StartDate,
    int InitialPopulation,
    int MaleCount,
    int FemaleCount,
    int UnsexedCount,
    int Population,
    string? Shed
)
{
    /// <summary>
    /// Parameterless constructor for mapping dto instance from core model/entity.
    /// </summary>
    public BatchDto() : this(Guid.Empty, string.Empty, null, string.Empty, string.Empty, 0, 0, 0, 0, 0, null)
    {

    }

    public BatchDto Map(Batch from, BatchDto? to = null)
    {
        return to is not null ? to with
        {
            Id = from.Id,
            Name = from.Name,
            Breed = from.Breed,
            Status = from.Status.ToString(),
            StartDate = from.StartDate.ToString(Constants.DateTimeFormat),
            InitialPopulation = from.InitialPopulation,
            MaleCount = from.MaleCount,
            FemaleCount = from.FemaleCount,
            UnsexedCount = from.UnsexedCount,
            Population = from.Population,
            Shed = from.Shed
        } : new BatchDto(
            from.Id,
            from.Name,
            from.Breed,
            from.Status.ToString(),
            from.StartDate.ToString(Constants.DateTimeFormat),
            from.InitialPopulation,
            from.MaleCount,
            from.FemaleCount,
            from.UnsexedCount,
            from.Population,
            from.Shed
        );
    }
}

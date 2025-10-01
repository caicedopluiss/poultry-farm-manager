using System;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Application.DTOs;

public record NewBatchDto(
    string Name,
    string StartClientDateIsoString,
    int MaleCount,
    int FemaleCount,
    int UnsexedCount,
    string? Breed) : IDtoEntityMapper<NewBatchDto, Batch>
{
    public Batch Map(NewBatchDto from, Batch? to = null)
    {
        var result = to ?? new();

        result.Name = from.Name;
        result.StartDate = Utils.ParseIso8601DateTimeString(from.StartClientDateIsoString).UtcDateTime;
        result.MaleCount = from.MaleCount;
        result.FemaleCount = from.FemaleCount;
        result.UnsexedCount = from.UnsexedCount;
        result.Breed = from.Breed;

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
    int Population
) : IDtoEntityMapper<Batch, BatchDto>
{
    /// <summary>
    /// Parameterless constructor for mapping dto instance from core model/entity.
    /// </summary>
    public BatchDto() : this(Guid.Empty, string.Empty, null, string.Empty, string.Empty, 0, 0, 0, 0, 0)
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
            Population = from.Population
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
            from.Population
        );
    }
}

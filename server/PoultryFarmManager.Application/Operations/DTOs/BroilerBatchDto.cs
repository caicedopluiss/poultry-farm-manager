using System;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Application.Operations.DTOs;

public record NewBroilerBatchDto
{
    public string BatchName { get; set; } = string.Empty;
    public string? Breed { get; set; }
    /// <summary>
    /// ISO 8601 date format
    /// </summary>
    public string? StartClientDate { get; set; }
    public int InitialPopulation { get; set; }
    public string Status { get; set; } = nameof(BroilerBatchStatus.Draft);
    public string? Notes { get; set; } = string.Empty;

    public virtual BroilerBatch ToCoreModel() => new()
    {
        BatchName = BatchName,
        Breed = Breed,
        StartDate = string.IsNullOrEmpty(StartClientDate?.Trim()) ? Utils.ParseIso8601DateTimeString(StartClientDate!).UtcDateTime : null,
        InitialPopulation = InitialPopulation,
        Status = Enum.Parse<BroilerBatchStatus>(Status),
        Notes = Notes
    };
}

public record UpdateBroilerBatchDto : NewBroilerBatchDto
{
    public int CurrentPopulation { get; set; }
    public string? ProcessingStartClientDate { get; set; }
    public string? ProcessingEndClientDate { get; set; }

    public override BroilerBatch ToCoreModel()
    {
        var batch = base.ToCoreModel();
        batch.CurrentPopulation = CurrentPopulation;
        batch.ProcessingStartDate = !string.IsNullOrEmpty(ProcessingStartClientDate?.Trim())
            ? Utils.ParseIso8601DateTimeString(ProcessingStartClientDate!).UtcDateTime
            : null;
        batch.ProcessingEndDate = !string.IsNullOrEmpty(ProcessingEndClientDate?.Trim())
            ? Utils.ParseIso8601DateTimeString(ProcessingEndClientDate!).UtcDateTime
            : null;
        return batch;
    }
}

public record BroilerBatchDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public string BatchName { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public DateTime? StartDate { get; set; }
    public int InitialPopulation { get; set; }
    public int CurrentPopulation { get; set; }
    public string Status { get; set; } = nameof(BroilerBatchStatus.Draft);
    public DateTime? ProcessingStartDate { get; set; }
    public DateTime? ProcessingEndDate { get; set; }
    public string? Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    public static BroilerBatchDto FromCore(BroilerBatch batch) => new()
    {
        Id = batch.Id,
        BatchName = batch.BatchName,
        Breed = batch.Breed,
        StartDate = batch.StartDate,
        InitialPopulation = batch.InitialPopulation,
        CurrentPopulation = batch.CurrentPopulation,
        Status = batch.Status.ToString(),
        ProcessingStartDate = batch.ProcessingStartDate,
        ProcessingEndDate = batch.ProcessingEndDate,
        Notes = batch.Notes,
        CreatedAt = batch.CreatedAt,
        ModifiedAt = batch.ModifiedAt
    };
}
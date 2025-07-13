using System;
using PoultryFarmManager.Application.Finances.DTOs;
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

    public virtual BroilerBatch ToCoreModel(BroilerBatch? baseBatch = null)
    {
        var result = baseBatch ?? new();

        result.BatchName = BatchName;
        result.Breed = Breed;
        result.StartDate = !string.IsNullOrWhiteSpace(StartClientDate) ?
            Utils.ParseIso8601DateTimeString(StartClientDate).UtcDateTime :
            null;
        result.InitialPopulation = InitialPopulation;
        result.Status = Enum.Parse<BroilerBatchStatus>(Status);
        result.Notes = Notes;

        return result;
    }
}

public record UpdateBroilerBatchDto : NewBroilerBatchDto
{
    public int CurrentPopulation { get; set; }
    public string? ProcessingStartClientDate { get; set; }
    public string? ProcessingEndClientDate { get; set; }

    public override BroilerBatch ToCoreModel(BroilerBatch? baseBatch = null)
    {
        // Only update the properties that are relevant for an update

        var result = base.ToCoreModel(baseBatch);

        result.CurrentPopulation = CurrentPopulation;
        result.ProcessingStartDate = !string.IsNullOrWhiteSpace(ProcessingStartClientDate)
            ? Utils.ParseIso8601DateTimeString(ProcessingStartClientDate).UtcDateTime
            : null;
        result.ProcessingEndDate = !string.IsNullOrWhiteSpace(ProcessingEndClientDate)
            ? Utils.ParseIso8601DateTimeString(ProcessingEndClientDate).UtcDateTime
            : null;

        return result;
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
    public FinancialTransactionDto? FinancialTransaction { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    public ActivityDto? LastWeightActivity { get; private set; }

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
        FinancialTransaction = batch.FinancialTransaction is not null
            ? FinancialTransactionDto.FromCore(batch.FinancialTransaction)
            : null,
        CreatedAt = batch.CreatedAt,
        ModifiedAt = batch.ModifiedAt
    };

    public void SetLastWeightActivity(ActivityDto activity)
    {
        LastWeightActivity = activity;
    }
}
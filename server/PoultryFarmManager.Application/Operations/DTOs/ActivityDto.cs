using System;
using System.Globalization;
using PoultryFarmManager.Core.Operations;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Application.Operations.DTOs;

public class NewActivityDto
{
    public Guid BroilerBatchId { get; set; }
    public string? Description { get; set; }
    public string Date { get; set; } = string.Empty;
    public decimal? Value { get; set; }
    public string? Unit { get; set; }
    public string Type { get; set; } = string.Empty;

    public Activity ToCore() => new()
    {
        BroilerBatchId = BroilerBatchId,
        Description = Description,
        Date = DateTime.ParseExact(Date, Constants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None),
        Value = Value is not null ? Utils.TruncateToTwoDecimals(Value.Value) : null,
        Unit = Unit,
        Type = Enum.Parse<ActivityType>(Type, true),
    };
}

public class ActivityDto
{
    public Guid Id { get; set; }
    public Guid BroilerBatchId { get; set; }
    public string? Description { get; set; }
    public string Date { get; set; } = string.Empty;
    public decimal? Value { get; set; }
    public string? Unit { get; set; }
    public string Type { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string? ModifiedAt { get; set; }

    public static ActivityDto FromCore(Activity activity) => new()
    {
        Id = activity.Id,
        BroilerBatchId = activity.BroilerBatchId,
        Description = activity.Description,
        Date = activity.Date.ToString(Constants.DateTimeFormat),
        Value = activity.Value,
        Unit = activity.Unit,
        Type = activity.Type.ToString(),
        CreatedAt = activity.CreatedAt.ToString(Constants.DateTimeFormat),
        ModifiedAt = activity.ModifiedAt?.ToString(Constants.DateTimeFormat),
    };
}
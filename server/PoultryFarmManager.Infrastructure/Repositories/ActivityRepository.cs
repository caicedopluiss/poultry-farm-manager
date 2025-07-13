using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Application.Operations.Repositories;
using PoultryFarmManager.Core.Operations;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Infrastructure.Repositories;

public class ActivityRepository(ApplicationDbContext dbContext) : IActivityRepository
{
    public async Task<IReadOnlyCollection<Activity>> GetActivitiesAsync(
        Guid broilerBatchId,
        (DateTimeOffset fromClientDate, DateTimeOffset toClientDate)? dateRange = null,
        ActivityType? activityType = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Activities
            .AsNoTracking()
            .Where(a => a.BroilerBatchId == broilerBatchId);

        if (dateRange is not null && dateRange.Value.fromClientDate != default && dateRange.Value.toClientDate != default)
        {
            // Get Utc DateTime from Client Date (DateTimeOffset) at 12AM from day.
            // In this case a new instance was required because .Date property doesn't store offset information for converting to Utc later
            var fromDate = new DateTimeOffset(dateRange.Value.fromClientDate.Date, dateRange.Value.fromClientDate.Offset).UtcDateTime;
            var toDate = new DateTimeOffset(dateRange.Value.toClientDate.Date, dateRange.Value.toClientDate.Offset).UtcDateTime;

            query = query.Where(a => a.Date >= fromDate && a.Date < toDate);
        }

        if (activityType.HasValue)
        {
            query = query.Where(a => a.Type == activityType.Value);
        }

        var result = await query.ToListAsync(cancellationToken);

        return result.AsReadOnly();
    }

    public async Task<Activity> AddActivityAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        return (await dbContext.Activities.AddAsync(activity, cancellationToken)).Entity;
    }
}
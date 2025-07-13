using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Operations;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Application.Operations.Repositories;

public interface IActivityRepository
{
    Task<IReadOnlyCollection<Activity>> GetActivitiesAsync(
        Guid broilerBatchId,
        (DateTimeOffset fromClientDate,
        DateTimeOffset toClientDate)? dateRange = null,
        ActivityType? activityType = null,
        CancellationToken cancellationToken = default);
    Task<Activity> AddActivityAsync(Activity activity, CancellationToken cancellationToken = default);
}
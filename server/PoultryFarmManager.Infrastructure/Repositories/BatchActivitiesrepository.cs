using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.Core.Models.BatchActivities;

namespace PoultryFarmManager.Infrastructure.Repositories;

public class BatchActivitiesRepository(AppDbContext context) : IBatchActivitiesRepository
{
    public async Task<IReadOnlyCollection<BatchActivity>> GetAllByBatchIdAsync(Guid batchId, BatchActivityType? type = null, CancellationToken cancellationToken = default)
    {
        if (type is null)
        {
            // Fetch all activity types separately to preserve all derived properties (Sex, NumberOfDeaths, NewStatus, etc.)
            // EF Core's TPT pattern with polymorphism doesn't support efficient UNION queries across different entity types
            var mortalityActivities = await context.MortalityRegistrationActivities.AsNoTracking()
                .Where(ba => ba.BatchId == batchId)
                .OrderByDescending(ba => ba.Date)
                .ToListAsync(cancellationToken);

            var statusSwitchActivities = await context.StatusSwitchActivities.AsNoTracking()
                .Where(ba => ba.BatchId == batchId)
                .OrderByDescending(ba => ba.Date)
                .ToListAsync(cancellationToken);

            var productConsumptionActivities = await context.ProductConsumptionActivities.AsNoTracking()
                .Include(ba => ba.Product)
                .Where(ba => ba.BatchId == batchId)
                .OrderByDescending(ba => ba.Date)
                .ToListAsync(cancellationToken);

            // Combine and sort in memory - this preserves all derived type properties
            var allActivities = mortalityActivities
                .Cast<BatchActivity>()
                .Concat(statusSwitchActivities)
                .Concat(productConsumptionActivities)
                .OrderByDescending(ba => ba.Date)
                .ToList();

            return allActivities;
        }
        else
        {
            IQueryable<BatchActivity> query = type.Value switch
            {
                BatchActivityType.MortalityRecording => context.MortalityRegistrationActivities.AsNoTracking()
                    .Where(ba => ba.BatchId == batchId),
                BatchActivityType.StatusSwitch => context.StatusSwitchActivities.AsNoTracking()
                    .Where(ba => ba.BatchId == batchId),
                BatchActivityType.ProductConsumption => context.ProductConsumptionActivities.AsNoTracking()
                    .Include(ba => ba.Product)
                    .Where(ba => ba.BatchId == batchId),
                _ => throw new NotSupportedException($"Batch activity type '{type.Value}' is not supported for retrieval.")
            };

            return await query
                .OrderByDescending(ba => ba.Date)
                .ToListAsync(cancellationToken);
        }
    }

    public async Task<BatchActivity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Try to find in mortality activities first
        var mortalityActivity = await context.MortalityRegistrationActivities.AsNoTracking()
            .FirstOrDefaultAsync(ba => ba.Id == id, cancellationToken);

        if (mortalityActivity is not null) return mortalityActivity;

        // Try to find in status switch activities
        var statusSwitchActivity = await context.StatusSwitchActivities.AsNoTracking()
            .FirstOrDefaultAsync(ba => ba.Id == id, cancellationToken);

        if (statusSwitchActivity is not null) return statusSwitchActivity;

        // Try to find in product consumption activities
        var productConsumptionActivity = await context.ProductConsumptionActivities.AsNoTracking()
            .Include(ba => ba.Product)
            .FirstOrDefaultAsync(ba => ba.Id == id, cancellationToken);

        return productConsumptionActivity;
    }

    public Task<BatchActivity> CreateAsync(BatchActivity batchActivity, CancellationToken cancellationToken = default)
    {
        BatchActivity created = batchActivity.Type switch
        {
            BatchActivityType.MortalityRecording => context.MortalityRegistrationActivities.Add((MortalityRegistrationBatchActivity)batchActivity).Entity,
            BatchActivityType.StatusSwitch => context.StatusSwitchActivities.Add((StatusSwitchBatchActivity)batchActivity).Entity,
            BatchActivityType.ProductConsumption => context.ProductConsumptionActivities.Add((ProductConsumptionBatchActivity)batchActivity).Entity,
            _ => throw new NotSupportedException($"Batch activity type '{batchActivity.Type}' is not supported for creation.")
        };
        return Task.FromResult(created);
    }
}

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
        // TODO: Implement filtering by type when more types are added
        var query = context.MortalityRegistrationActivities.AsNoTracking()
            .Where(ba => ba.BatchId == batchId);

        return await query
            .OrderByDescending(ba => ba.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<BatchActivity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.MortalityRegistrationActivities.AsNoTracking()
            .FirstOrDefaultAsync(ba => ba.Id == id, cancellationToken);
    }

    public Task<BatchActivity> CreateAsync(BatchActivity batchActivity, CancellationToken cancellationToken = default)
    {
        BatchActivity created = batchActivity.Type switch
        {
            BatchActivityType.MortalityRecording => context.MortalityRegistrationActivities.Add((MortalityRegistrationBatchActivity)batchActivity).Entity,
            _ => throw new NotSupportedException($"Batch activity type '{batchActivity.Type}' is not supported for creation.")
        };
        return Task.FromResult(created);
    }
}

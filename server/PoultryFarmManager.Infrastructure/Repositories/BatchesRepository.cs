using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Infrastructure.Repositories;

public sealed class BatchesRepository(AppDbContext context) : IBatchesRepository
{
    public Task<Batch> CreateAsync(Batch batch, CancellationToken cancellationToken = default)
    {
        var createdBatch = context.Batches.Add(batch).Entity;
        return Task.FromResult(createdBatch);
    }

    public async Task<IReadOnlyCollection<Batch>> GetAllAsync(string? sortBy = null, string? sortOrder = null, CancellationToken cancellationToken = default)
    {
        var query = context.Batches.AsNoTracking();

        var isDescending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.Equals(sortBy, "name", StringComparison.OrdinalIgnoreCase))
        {
            query = isDescending ? query.OrderByDescending(b => b.Name) : query.OrderBy(b => b.Name);
        }
        else if (string.Equals(sortBy, "status", StringComparison.OrdinalIgnoreCase))
        {
            query = isDescending ? query.OrderByDescending(b => b.Status) : query.OrderBy(b => b.Status);
        }
        else
        {
            // Default sorting
            query = query.OrderByDescending(b => b.StartDate);
        }

        var batches = await query.ToListAsync(cancellationToken);
        return batches;
    }

    public async Task<Batch?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = context.Batches
            .Include(b => b.FeedingTable)
            .ThenInclude(t => t!.DayEntries)
            .AsQueryable();

        if (!track)
        {
            query = query.AsNoTracking();
        }

        var batch = await query.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        return batch;
    }

    public async Task<Batch?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var batch = await context.Batches.AsNoTracking().FirstOrDefaultAsync(b => b.Name == name, cancellationToken);
        return batch;
    }
}

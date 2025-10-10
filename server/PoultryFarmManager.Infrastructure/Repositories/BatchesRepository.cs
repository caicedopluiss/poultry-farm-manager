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

    public async Task<IReadOnlyCollection<Batch>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var batches = await context.Batches.AsNoTracking().OrderByDescending(b => b.StartDate).ToListAsync(cancellationToken);
        return batches;
    }

    public async Task<Batch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var batch = await context.Batches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        return batch;
    }
}

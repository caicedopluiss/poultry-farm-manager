using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Application.Operations.Repositories;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Infrastructure.Repositories;

public class BroilerBatchRepository(ApplicationDbContext dbContext) : IBroilerBatchRepository
{
    public async Task<BroilerBatch> AddAsync(BroilerBatch batch, CancellationToken cancellationToken = default)
    {
        var result = (await dbContext.BroilerBatches.AddAsync(batch, cancellationToken)).Entity;

        return result;
    }

    public async Task<IReadOnlyCollection<BroilerBatch>> GetAllAsync(bool includeFinancialTransaction = false, CancellationToken cancellationToken = default)
    {
        var query = dbContext.BroilerBatches.AsQueryable();
        if (includeFinancialTransaction) query = query.Include(b => b.FinancialTransaction).ThenInclude(ft => ft!.FinancialEntity);
        var batches = await query.AsNoTracking().ToListAsync(cancellationToken);
        return batches.AsReadOnly();
    }

    public Task<BroilerBatch?> GetByIdAsync(Guid id, bool track = false, bool includeFinancialTransaction = false, CancellationToken cancellationToken = default)
    {
        var query = dbContext.BroilerBatches.AsQueryable();
        if (!track) query = query.AsNoTracking();
        if (includeFinancialTransaction) query = query.Include(b => b.FinancialTransaction).ThenInclude(ft => ft!.FinancialEntity);
        return query.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public Task UpdateAsync(BroilerBatch batch, CancellationToken cancellationToken = default)
    {
        var local = dbContext.BroilerBatches.Local.FirstOrDefault(b => b.Id == batch.Id);
        if (local is not null)
        {
            dbContext.Entry(local).CurrentValues.SetValues(batch);
        }
        else
        {
            dbContext.BroilerBatches.Attach(batch);
            dbContext.Entry(batch).State = EntityState.Modified;
        }

        dbContext.Entry(batch).Property(x => x.Id).IsModified = false;
        dbContext.Entry(batch).Property(x => x.CreatedAt).IsModified = false;

        return Task.CompletedTask;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Infrastructure.Repositories;

public sealed class SaleOrdersRepository(AppDbContext context) : ISaleOrdersRepository
{
    public Task<SaleOrder> CreateAsync(SaleOrder saleOrder, CancellationToken cancellationToken = default)
    {
        var created = context.SaleOrders.Add(saleOrder).Entity;
        return Task.FromResult(created);
    }

    public async Task<SaleOrder?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = context.SaleOrders
            .Include(so => so.Batch)
            .Include(so => so.Customer)
            .Include(so => so.Items)
            .Include(so => so.Payments)
            .AsQueryable();

        if (!track)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(so => so.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<SaleOrder>> GetByBatchIdAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        return await context.SaleOrders
            .Where(so => so.BatchId == batchId)
            .Include(so => so.Batch)
            .Include(so => so.Customer)
            .Include(so => so.Items)
            .Include(so => so.Payments)
            .AsNoTracking()
            .OrderByDescending(so => so.Date)
            .ToListAsync(cancellationToken);
    }

    public Task<SaleOrder> UpdateAsync(SaleOrder saleOrder, CancellationToken cancellationToken = default)
    {
        var updated = context.SaleOrders.Update(saleOrder).Entity;
        return Task.FromResult(updated);
    }
}

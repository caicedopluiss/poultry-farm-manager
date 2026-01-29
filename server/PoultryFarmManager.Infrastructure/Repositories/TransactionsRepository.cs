using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Infrastructure.Repositories;

public sealed class TransactionsRepository(AppDbContext context) : ITransactionsRepository
{
    public Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var createdTransaction = context.Transactions.Add(transaction).Entity;
        return Task.FromResult(createdTransaction);
    }

    public async Task<IReadOnlyCollection<Transaction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var transactions = await context.Transactions
            .Include(t => t.ProductVariant)
            .Include(t => t.Batch)
            .Include(t => t.Vendor)
            .Include(t => t.Customer)
            .AsNoTracking()
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);
        return transactions;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = context.Transactions
            .Include(t => t.ProductVariant)
            .Include(t => t.Batch)
            .Include(t => t.Vendor)
            .Include(t => t.Customer)
            .AsQueryable();

        if (!track)
        {
            query = query.AsNoTracking();
        }

        var transaction = await query.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        return transaction;
    }

    public Task<Transaction> UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var updatedTransaction = context.Transactions.Update(transaction).Entity;
        return Task.FromResult(updatedTransaction);
    }
}

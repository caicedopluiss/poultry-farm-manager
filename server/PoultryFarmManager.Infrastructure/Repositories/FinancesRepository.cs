using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Application.Finances;
using PoultryFarmManager.Core.Finances.Models;

namespace PoultryFarmManager.Infrastructure.Repositories;

public class Finances(ApplicationDbContext dbContext) : IFinancesRepository
{
    public Task<FinancialEntity?> GetFinancialEntityByIdAsync(Guid financialEntityId, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = dbContext.FinancialEntities.AsQueryable();
        if (!track) query = query.AsNoTracking();

        return query.FirstOrDefaultAsync(e => e.Id == financialEntityId, cancellationToken);
    }

    public async Task<FinancialEntity> AddFinancialEntityAsync(FinancialEntity financialEntity, CancellationToken cancellationToken = default)
    {
        var result = (await dbContext.FinancialEntities.AddAsync(financialEntity, cancellationToken)).Entity;
        return result;
    }

    public async Task<FinancialTransaction> AddFinancialTransactionAsync(FinancialTransaction financialTransaction, CancellationToken cancellationToken = default)
    {
        var result = (await dbContext.FinancialTransactions.AddAsync(financialTransaction, cancellationToken)).Entity;
        return result;
    }
}
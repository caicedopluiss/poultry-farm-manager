using System;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Finances.Models;

namespace PoultryFarmManager.Application.Finances;

public interface IFinancesRepository
{
    public Task<FinancialEntity?> GetFinancialEntityByIdAsync(Guid financialEntityId, bool track = false, CancellationToken cancellationToken = default);
    public Task<FinancialEntity> AddFinancialEntityAsync(FinancialEntity financialEntity, CancellationToken cancellationToken = default);
    public Task<FinancialTransaction> AddFinancialTransactionAsync(FinancialTransaction financialTransaction, CancellationToken cancellationToken = default);
}
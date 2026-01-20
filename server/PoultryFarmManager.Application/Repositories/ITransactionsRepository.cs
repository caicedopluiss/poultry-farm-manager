using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Application.Repositories;

public interface ITransactionsRepository
{
    Task<IReadOnlyCollection<Transaction>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Transaction?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default);
    Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<Transaction> UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Application.Operations.Repositories;

public interface IBroilerBatchRepository
{
    Task<BroilerBatch> AddAsync(BroilerBatch batch, CancellationToken cancellationToken = default);
    Task<BroilerBatch?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BroilerBatch>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(BroilerBatch batch, CancellationToken cancellationToken = default);
}
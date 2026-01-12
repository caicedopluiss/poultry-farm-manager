using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Application.Repositories;

public interface IBatchActivitiesRepository
{
    Task<IReadOnlyCollection<BatchActivity>> GetAllByBatchIdAsync(Guid batchId, BatchActivityType? type = null, CancellationToken cancellationToken = default);
    Task<BatchActivity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BatchActivity> CreateAsync(BatchActivity batchActivity, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, DateTime?>> GetFirstStatusSwitchByBatchIdsAsync(IEnumerable<Guid> batchIds, CancellationToken cancellationToken = default);
}

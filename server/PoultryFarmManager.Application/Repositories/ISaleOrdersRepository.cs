using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Application.Repositories;

public interface ISaleOrdersRepository
{
    Task<SaleOrder?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SaleOrder>> GetByBatchIdAsync(Guid batchId, CancellationToken cancellationToken = default);
    Task<SaleOrder> CreateAsync(SaleOrder saleOrder, CancellationToken cancellationToken = default);
    Task<SaleOrder> UpdateAsync(SaleOrder saleOrder, CancellationToken cancellationToken = default);
}

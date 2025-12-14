using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Application.Repositories;

public interface IAssetsRepository
{
    Task<IReadOnlyCollection<Asset>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Asset?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default);
    Task<Asset> CreateAsync(Asset asset, CancellationToken cancellationToken = default);
    Task<Asset> UpdateAsync(Asset asset, CancellationToken cancellationToken = default);
}

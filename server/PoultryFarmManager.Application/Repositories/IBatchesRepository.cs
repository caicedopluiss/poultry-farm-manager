using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Application.Repositories;

public interface IBatchesRepository
{
    Task<IReadOnlyCollection<Batch>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Batch?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default);
    Task<Batch?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Batch> CreateAsync(Batch batch, CancellationToken cancellationToken = default);
}

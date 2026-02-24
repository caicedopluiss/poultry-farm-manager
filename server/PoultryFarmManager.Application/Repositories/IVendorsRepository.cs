using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Application.Repositories;

public interface IVendorsRepository
{
    Task<IReadOnlyCollection<Vendor>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Vendor?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default);
    Task<Vendor> CreateAsync(Vendor vendor, CancellationToken cancellationToken = default);
    Task<Vendor> UpdateAsync(Vendor vendor, CancellationToken cancellationToken = default);
}

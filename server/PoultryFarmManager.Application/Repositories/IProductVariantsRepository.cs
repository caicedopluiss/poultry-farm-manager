using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Application.Repositories;

public interface IProductVariantsRepository
{
    Task<IReadOnlyCollection<ProductVariant>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductVariant>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductVariant?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default);
    Task<ProductVariant> CreateAsync(ProductVariant productVariant, CancellationToken cancellationToken = default);
    Task<ProductVariant> UpdateAsync(ProductVariant productVariant, CancellationToken cancellationToken = default);
}

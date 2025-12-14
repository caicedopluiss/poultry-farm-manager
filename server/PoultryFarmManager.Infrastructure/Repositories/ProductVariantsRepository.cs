using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Infrastructure.Repositories;

public sealed class ProductVariantsRepository(AppDbContext context) : IProductVariantsRepository
{
    public Task<ProductVariant> CreateAsync(ProductVariant productVariant, CancellationToken cancellationToken = default)
    {
        var createdProductVariant = context.ProductVariants.Add(productVariant).Entity;
        return Task.FromResult(createdProductVariant);
    }

    public async Task<IReadOnlyCollection<ProductVariant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var productVariants = await context.ProductVariants
            .AsNoTracking()
            .Include(pv => pv.Product)
            .OrderBy(pv => pv.Name)
            .ToListAsync(cancellationToken);
        return productVariants;
    }

    public async Task<IReadOnlyCollection<ProductVariant>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var productVariants = await context.ProductVariants
            .AsNoTracking()
            .Where(pv => pv.ProductId == productId)
            .OrderBy(pv => pv.Name)
            .ToListAsync(cancellationToken);
        return productVariants;
    }

    public async Task<ProductVariant?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = context.ProductVariants.Include(pv => pv.Product).AsQueryable();

        if (!track)
        {
            query = query.AsNoTracking();
        }

        var productVariant = await query.FirstOrDefaultAsync(pv => pv.Id == id, cancellationToken);
        return productVariant;
    }

    public Task<ProductVariant> UpdateAsync(ProductVariant productVariant, CancellationToken cancellationToken = default)
    {
        var updatedProductVariant = context.ProductVariants.Update(productVariant).Entity;
        return Task.FromResult(updatedProductVariant);
    }
}

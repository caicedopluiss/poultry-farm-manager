using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Infrastructure.Repositories;

public sealed class ProductsRepository(AppDbContext context) : IProductsRepository
{
    public Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var createdProduct = context.Products.Add(product).Entity;
        return Task.FromResult(createdProduct);
    }

    public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
        return products;
    }

    public async Task<Product?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = context.Products
            .Include(p => p.Variants)
            .AsQueryable();

        if (!track)
        {
            query = query.AsNoTracking();
        }

        var product = await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return product;
    }

    public Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var updatedProduct = context.Products.Update(product).Entity;
        return Task.FromResult(updatedProduct);
    }
}

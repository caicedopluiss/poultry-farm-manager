using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Infrastructure.Repositories;

public sealed class AssetsRepository(AppDbContext context) : IAssetsRepository
{
    public Task<Asset> CreateAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        var createdAsset = context.Assets.Add(asset).Entity;
        return Task.FromResult(createdAsset);
    }

    public async Task<IReadOnlyCollection<Asset>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var assets = await context.Assets
            .Include(a => a.States)
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
        return assets;
    }

    public async Task<Asset?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = context.Assets
            .Include(a => a.States)
            .AsQueryable();

        if (!track)
        {
            query = query.AsNoTracking();
        }

        var asset = await query.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        return asset;
    }

    public Task<Asset> UpdateAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        var updatedAsset = context.Assets.Update(asset).Entity;
        return Task.FromResult(updatedAsset);
    }
}

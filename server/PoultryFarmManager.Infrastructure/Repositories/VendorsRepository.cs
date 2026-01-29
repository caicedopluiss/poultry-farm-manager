using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Infrastructure.Repositories;

public sealed class VendorsRepository(AppDbContext context) : IVendorsRepository
{
    public Task<Vendor> CreateAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        var createdVendor = context.Vendors.Add(vendor).Entity;
        return Task.FromResult(createdVendor);
    }

    public async Task<IReadOnlyCollection<Vendor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var vendors = await context.Vendors
            .Include(v => v.ContactPerson)
            .AsNoTracking()
            .OrderBy(v => v.Name)
            .ToListAsync(cancellationToken);
        return vendors;
    }

    public async Task<Vendor?> GetByIdAsync(Guid id, bool track = false, CancellationToken cancellationToken = default)
    {
        var query = context.Vendors
            .Include(v => v.ContactPerson)
            .AsQueryable();

        if (!track)
        {
            query = query.AsNoTracking();
        }

        var vendor = await query.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        return vendor;
    }

    public Task<Vendor> UpdateAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        var updatedVendor = context.Vendors.Update(vendor).Entity;
        return Task.FromResult(updatedVendor);
    }
}

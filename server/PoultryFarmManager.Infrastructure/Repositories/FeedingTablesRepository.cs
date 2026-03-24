using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Infrastructure.Repositories;

public sealed class FeedingTablesRepository(AppDbContext context) : IFeedingTablesRepository
{
    public async Task<IReadOnlyCollection<FeedingTable>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.FeedingTables
            .Include(t => t.DayEntries)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<FeedingTable?> GetByIdAsync(Guid id, bool track = false, CancellationToken ct = default)
    {
        var query = context.FeedingTables
            .Include(t => t.DayEntries)
            .AsQueryable();

        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<FeedingTable?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await context.FeedingTables
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name == name, ct);
    }

    public async Task<FeedingTable> CreateAsync(FeedingTable feedingTable, CancellationToken ct = default)
    {
        var created = context.FeedingTables.Add(feedingTable).Entity;
        return await Task.FromResult(created);
    }

    public void DeleteDayEntries(IEnumerable<FeedingTableDayEntry> entries)
    {
        context.FeedingTableDayEntries.RemoveRange(entries);
    }
}

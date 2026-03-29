using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Application.Repositories;

public interface IFeedingTablesRepository
{
    Task<IReadOnlyCollection<FeedingTable>> GetAllAsync(CancellationToken ct = default);
    Task<FeedingTable?> GetByIdAsync(Guid id, bool track = false, CancellationToken ct = default);
    Task<FeedingTable?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<FeedingTable> CreateAsync(FeedingTable feedingTable, CancellationToken ct = default);
    void DeleteDayEntries(IEnumerable<FeedingTableDayEntry> entries);
}

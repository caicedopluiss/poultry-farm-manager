using System;
using System.Collections.Generic;

namespace PoultryFarmManager.Core.Models;

public class FeedingTable : DbEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<FeedingTableDayEntry> DayEntries { get; set; } = [];
}

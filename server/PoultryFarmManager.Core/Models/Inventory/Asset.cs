using System.Collections.Generic;

namespace PoultryFarmManager.Core.Models.Inventory
{
    public class Asset : DbEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }

        public ICollection<AssetState>? States { get; set; }
    }
}

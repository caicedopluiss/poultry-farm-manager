using System;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Core.Models.Inventory
{
    public class AssetState : DbEntity
    {
        public Guid AssetId { get; set; }
        public AssetStatus Status { get; set; }
        public int Quantity { get; set; }
        public string? Location { get; set; }

        public Asset? Asset { get; set; }
    }
}

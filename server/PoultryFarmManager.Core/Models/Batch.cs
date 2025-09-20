using System;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Core.Models
{
    public class Batch : DbEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Breed { get; set; }
        public BatchStatus Status { get; set; } = BatchStatus.Planned;
        public DateTime StartDate { get; set; }
        public int InitialPopulation { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public int UnsexedCount { get; set; }
        public int Population => MaleCount + FemaleCount + UnsexedCount;
    }
}

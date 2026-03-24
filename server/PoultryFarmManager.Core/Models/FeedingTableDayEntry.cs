using System;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Core.Models;

public class FeedingTableDayEntry : DbEntity
{
    public Guid FeedingTableId { get; set; }
    public int DayNumber { get; set; }
    public FoodType FoodType { get; set; }
    public decimal AmountPerBird { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; }
    public decimal? ExpectedBirdWeight { get; set; }
    public UnitOfMeasure? ExpectedBirdWeightUnitOfMeasure { get; set; }
    public FeedingTable? FeedingTable { get; set; }
}

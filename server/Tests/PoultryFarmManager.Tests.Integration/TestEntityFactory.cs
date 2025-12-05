using System;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core;

namespace PoultryFarmManager.Tests.Integration;

internal static class TestEntityFactory
{
    // Example usage: TestEntityFactory.GetFactory<Batch>().CreateRandom();
    internal static IEntityFactory<T> GetFactory<T>() where T : IDbEntity
    {
        if (typeof(T) == typeof(Batch))
            return (IEntityFactory<T>)new BatchFactory();

        throw new NotSupportedException($"No factory registered for type {typeof(T).Name}");
    }
}

internal class BatchFactory : IEntityFactory<Batch>
{
    private static readonly Random _random = new();

    public Batch CreateRandom()
    {
        var breeds = new[] { null, "Leghorn", "Rhode Island Red", "Sussex", "Plymouth Rock", "Cornish" };
        var sheds = new[] { null, "Shed A-1", "Shed A-2", "Shed B-1", "Shed B-2", "Shed C-1", "Shed C-2", "Shed D-1" };
        var name = $"Batch_{_random.Next(1000, 9999)}";
        var breed = breeds[_random.Next(breeds.Length)];
        var shed = sheds[_random.Next(sheds.Length)];
        var startDate = DateTime.UtcNow.AddDays(_random.Next(-60, 30));
        var maleCount = _random.Next(10, 200);
        var femaleCount = _random.Next(10, 200);
        var unsexedCount = _random.Next(0, 50);
        var status = BatchStatus.Active;

        return new Batch
        {
            Name = name,
            Breed = breed,
            Shed = shed,
            StartDate = startDate,
            MaleCount = maleCount,
            FemaleCount = femaleCount,
            UnsexedCount = unsexedCount,
            Status = status,
            InitialPopulation = maleCount + femaleCount + unsexedCount
        };
    }
}

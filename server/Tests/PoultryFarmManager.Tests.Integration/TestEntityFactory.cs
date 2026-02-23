using System;
using System.Threading.Tasks;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.Core.Models.Inventory;
using PoultryFarmManager.Core.Models.Finance;
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

        if (typeof(T) == typeof(Asset))
            return (IEntityFactory<T>)new AssetFactory();

        if (typeof(T) == typeof(Product))
            return (IEntityFactory<T>)new ProductFactory();

        if (typeof(T) == typeof(ProductVariant))
            return (IEntityFactory<T>)new ProductVariantFactory();

        throw new NotSupportedException($"No factory registered for type {typeof(T).Name}");
    }

    /// <summary>
    /// Creates and persists a Vendor with a contact Person
    /// </summary>
    internal static async Task<Vendor> CreateVendorAsync(this TestsDbContext context)
    {
        var contactPerson = new Person
        {
            FirstName = $"Contact_{Guid.NewGuid().ToString()[..8]}",
            LastName = "Person",
            Email = $"contact_{Guid.NewGuid().ToString()[..8]}@vendor.com",
            PhoneNumber = "555-0100"
        };
        context.Persons.Add(contactPerson);
        await context.SaveChangesAsync();

        var vendor = new Vendor
        {
            Name = $"Vendor_{Guid.NewGuid().ToString()[..8]}",
            Location = "Test Location",
            ContactPersonId = contactPerson.Id
        };
        context.Vendors.Add(vendor);
        await context.SaveChangesAsync();

        return vendor;
    }

    /// <summary>
    /// Creates and persists an Asset with optional vendor and unit price
    /// </summary>
    internal static async Task<Asset> CreateAssetAsync(
        this TestsDbContext context,
        Guid? vendorId = null,
        decimal unitPrice = 100m,
        int quantity = 1)
    {
        var asset = new Asset
        {
            Name = $"Asset_{Guid.NewGuid().ToString()[..8]}",
            Description = "Test asset",
            States = [
                new AssetState
                {
                    Status = AssetStatus.Available,
                    Quantity = quantity,
                    Location = "Test Location"
                }
            ]
        };
        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        // Create initial transaction if vendor is provided
        if (vendorId.HasValue)
        {
            var transaction = new Transaction
            {
                Title = $"Initial purchase of {asset.Name}",
                Date = DateTime.UtcNow,
                Type = TransactionType.Expense,
                UnitPrice = unitPrice,
                Quantity = quantity,
                TransactionAmount = unitPrice * quantity,
                AssetId = asset.Id,
                VendorId = vendorId
            };
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();
        }

        return asset;
    }

    /// <summary>
    /// Creates and persists a Transaction for an Asset
    /// </summary>
    internal static async Task<Transaction> CreateTransactionAsync(
        this TestsDbContext context,
        Guid? assetId = null,
        Guid? vendorId = null,
        decimal unitPrice = 100m,
        DateTime? date = null,
        int quantity = 1)
    {
        var transaction = new Transaction
        {
            Title = $"Transaction_{Guid.NewGuid().ToString()[..8]}",
            Date = date ?? DateTime.UtcNow,
            Type = TransactionType.Expense,
            UnitPrice = unitPrice,
            Quantity = quantity,
            TransactionAmount = unitPrice * quantity,
            AssetId = assetId,
            VendorId = vendorId
        };
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        return transaction;
    }
}

internal class BatchFactory : IEntityFactory<Batch>
{
    private static readonly Random _random = new();

    public Batch CreateRandom()
    {
        var breeds = new[] { null, "Leghorn", "Rhode Island Red", "Sussex", "Plymouth Rock", "Cornish" };
        var sheds = new[] { null, "Shed A-1", "Shed A-2", "Shed B-1", "Shed B-2", "Shed C-1", "Shed C-2", "Shed D-1" };
        var name = $"Batch_{Guid.NewGuid().ToString()[..8]}";
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

internal class AssetFactory : IEntityFactory<Asset>
{
    private static readonly Random _random = new();

    public Asset CreateRandom()
    {
        var names = new[] { "Tractor", "Incubator", "Feeder", "Drinker", "Heater", "Generator", "Tool Kit", "Cage System" };
        var locations = new[] { null, "Warehouse A", "Shed B-1", "Maintenance Room", "Storage Unit 3", "Field Equipment Bay" };

        return new Asset
        {
            Name = $"{names[_random.Next(names.Length)]}_{Guid.NewGuid().ToString()[..8]}",
            Description = _random.Next(2) == 0 ? $"Test asset description {Guid.NewGuid().ToString()[..8]}" : null,
            Notes = _random.Next(3) == 0 ? $"Random notes {Guid.NewGuid().ToString()[..8]}" : null,
            States = [
                new AssetState
                {
                    Status = AssetStatus.Available,
                    Quantity = _random.Next(1, 10),
                    Location = locations[_random.Next(locations.Length)]
                }
            ]
        };
    }
}

internal class ProductFactory : IEntityFactory<Product>
{
    private static readonly Random _random = new();

    public Product CreateRandom()
    {
        var names = new[] { "Chicken Feed", "Corn", "Soy Meal", "Vitamins", "Antibiotics", "Disinfectant", "Bedding Material" };
        var manufacturers = new[] { "FarmCo", "AgriSupply Inc", "PoultryPro", "FeedMasters", "BioVet", "AgroTech" };
        var units = Enum.GetValues<UnitOfMeasure>();

        return new Product
        {
            Name = $"{names[_random.Next(names.Length)]}_{Guid.NewGuid().ToString()[..8]}",
            Manufacturer = manufacturers[_random.Next(manufacturers.Length)],
            UnitOfMeasure = units[_random.Next(units.Length)],
            Stock = Math.Round((decimal)(_random.NextDouble() * 1000), 2),
            Description = _random.Next(2) == 0 ? $"Test product description {Guid.NewGuid().ToString()[..8]}" : null
        };
    }
}

internal class ProductVariantFactory : IEntityFactory<ProductVariant>
{
    private static readonly Random _random = new();

    public ProductVariant CreateRandom()
    {
        var names = new[] { "Small Pack", "Medium Pack", "Large Pack", "Bulk", "Sample", "Premium", "Economy" };
        var units = Enum.GetValues<UnitOfMeasure>();

        return new ProductVariant
        {
            ProductId = Guid.NewGuid(), // This should be replaced with actual Product ID in tests
            Name = $"{names[_random.Next(names.Length)]}_{Guid.NewGuid().ToString()[..8]}",
            UnitOfMeasure = units[_random.Next(units.Length)],
            Stock = Math.Round((decimal)(_random.NextDouble() * 500), 2),
            Quantity = _random.Next(1, 100),
            Description = _random.Next(2) == 0 ? $"Test variant description {Guid.NewGuid().ToString()[..8]}" : null
        };
    }
}

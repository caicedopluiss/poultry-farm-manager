using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.ProductVariants;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.Core.Models.Finance;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetProductVariantPricingByVendorQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetProductVariantPricingByVendorQuery.Args, GetProductVariantPricingByVendorQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetProductVariantPricingByVendorQuery.Args, GetProductVariantPricingByVendorQuery.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetProductVariantPricingByVendorQuery_ShouldGroupByVendor_AndShowLastPrice()
    {
        // Arrange
        var person1 = new Person { FirstName = "John", LastName = "Supplier1" };
        var person2 = new Person { FirstName = "Jane", LastName = "Supplier2" };
        dbContext.Persons.AddRange(person1, person2);

        var vendor1 = new Vendor { Name = "Vendor A", ContactPersonId = person1.Id };
        var vendor2 = new Vendor { Name = "Vendor B", ContactPersonId = person2.Id };
        dbContext.Vendors.AddRange(vendor1, vendor2);

        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);

        var productVariant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "25kg Bag",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 2500m
        };
        dbContext.ProductVariants.Add(productVariant);

        // Multiple purchases from Vendor A
        var transaction1 = new Transaction
        {
            Title = "Purchase 1 from Vendor A",
            Date = DateTime.UtcNow.AddDays(-10),
            Type = TransactionType.Expense,
            UnitPrice = 15.00m,
            Quantity = 25,
            TransactionAmount = 375.00m,
            ProductVariantId = productVariant.Id,
            VendorId = vendor1.Id
        };

        var transaction2 = new Transaction
        {
            Title = "Purchase 2 from Vendor A",
            Date = DateTime.UtcNow.AddDays(-5),
            Type = TransactionType.Expense,
            UnitPrice = 16.00m,
            Quantity = 30,
            TransactionAmount = 480.00m,
            ProductVariantId = productVariant.Id,
            VendorId = vendor1.Id
        };

        var transaction3 = new Transaction
        {
            Title = "Latest Purchase from Vendor A",
            Date = DateTime.UtcNow.AddDays(-1),
            Type = TransactionType.Expense,
            UnitPrice = 17.50m, // Latest price from Vendor A
            Quantity = 20,
            TransactionAmount = 350.00m,
            ProductVariantId = productVariant.Id,
            VendorId = vendor1.Id
        };

        // Single purchase from Vendor B
        var transaction4 = new Transaction
        {
            Title = "Purchase from Vendor B",
            Date = DateTime.UtcNow.AddDays(-3),
            Type = TransactionType.Expense,
            UnitPrice = 14.50m,
            Quantity = 40,
            TransactionAmount = 580.00m,
            ProductVariantId = productVariant.Id,
            VendorId = vendor2.Id
        };

        dbContext.Transactions.AddRange(transaction1, transaction2, transaction3, transaction4);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetProductVariantPricingByVendorQuery.Args>(new(productVariant.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.VendorPricings.Count());

        var pricings = result.Value.VendorPricings.ToList();

        // Should be ordered by last purchase date descending
        var vendorAPricing = pricings.First(p => p.Vendor.Name == "Vendor A");
        Assert.Equal(17.50m, vendorAPricing.LastUnitPrice); // Latest price
        Assert.Equal(3, vendorAPricing.TotalPurchases);
        Assert.Equal("Vendor A", vendorAPricing.Vendor.Name);

        var vendorBPricing = pricings.First(p => p.Vendor.Name == "Vendor B");
        Assert.Equal(14.50m, vendorBPricing.LastUnitPrice);
        Assert.Equal(1, vendorBPricing.TotalPurchases);
        Assert.Equal("Vendor B", vendorBPricing.Vendor.Name);
    }

    [Fact]
    public async Task GetProductVariantPricingByVendorQuery_ShouldReturnEmpty_WhenNoTransactions()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);

        var productVariant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "25kg Bag",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 2500m
        };
        dbContext.ProductVariants.Add(productVariant);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetProductVariantPricingByVendorQuery.Args>(new(productVariant.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.VendorPricings);
    }

    [Fact]
    public async Task GetProductVariantPricingByVendorQuery_ShouldFail_WithEmptyProductVariantId()
    {
        // Arrange
        var request = new AppRequest<GetProductVariantPricingByVendorQuery.Args>(new(Guid.Empty));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "productVariantId");
    }

    [Fact]
    public async Task GetProductVariantPricingByVendorQuery_ShouldOrderByMostRecentPurchase()
    {
        // Arrange
        var person1 = new Person { FirstName = "John", LastName = "Doe" };
        var person2 = new Person { FirstName = "Jane", LastName = "Smith" };
        var person3 = new Person { FirstName = "Bob", LastName = "Johnson" };
        dbContext.Persons.AddRange(person1, person2, person3);

        var vendor1 = new Vendor { Name = "Old Vendor", ContactPersonId = person1.Id };
        var vendor2 = new Vendor { Name = "Recent Vendor", ContactPersonId = person2.Id };
        var vendor3 = new Vendor { Name = "Newest Vendor", ContactPersonId = person3.Id };
        dbContext.Vendors.AddRange(vendor1, vendor2, vendor3);

        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);

        var productVariant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "25kg Bag",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 2500m
        };
        dbContext.ProductVariants.Add(productVariant);

        var oldTransaction = new Transaction
        {
            Title = "Old Purchase",
            Date = DateTime.UtcNow.AddDays(-30),
            Type = TransactionType.Expense,
            UnitPrice = 10.00m,
            Quantity = 25,
            TransactionAmount = 250.00m,
            ProductVariantId = productVariant.Id,
            VendorId = vendor1.Id
        };

        var recentTransaction = new Transaction
        {
            Title = "Recent Purchase",
            Date = DateTime.UtcNow.AddDays(-5),
            Type = TransactionType.Expense,
            UnitPrice = 15.00m,
            Quantity = 25,
            TransactionAmount = 375.00m,
            ProductVariantId = productVariant.Id,
            VendorId = vendor2.Id
        };

        var newestTransaction = new Transaction
        {
            Title = "Newest Purchase",
            Date = DateTime.UtcNow.AddDays(-1),
            Type = TransactionType.Expense,
            UnitPrice = 16.00m,
            Quantity = 25,
            TransactionAmount = 400.00m,
            ProductVariantId = productVariant.Id,
            VendorId = vendor3.Id
        };

        dbContext.Transactions.AddRange(oldTransaction, recentTransaction, newestTransaction);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetProductVariantPricingByVendorQuery.Args>(new(productVariant.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        var pricings = result.Value!.VendorPricings.ToList();

        // Should be ordered by most recent purchase first
        Assert.Equal("Newest Vendor", pricings[0].Vendor.Name);
        Assert.Equal("Recent Vendor", pricings[1].Vendor.Name);
        Assert.Equal("Old Vendor", pricings[2].Vendor.Name);
    }

    [Fact]
    public async Task GetProductVariantPricingByVendorQuery_ShouldIncludeVendorContactPerson()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "Contact",
            LastName = "Person",
            Email = "contact@vendor.com",
            PhoneNumber = "123-456-7890"
        };
        dbContext.Persons.Add(person);

        var vendor = new Vendor { Name = "Test Vendor", ContactPersonId = person.Id, Location = "Test City" };
        dbContext.Vendors.Add(vendor);

        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);

        var productVariant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "25kg Bag",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 2500m
        };
        dbContext.ProductVariants.Add(productVariant);

        var transaction = new Transaction
        {
            Title = "Purchase",
            Date = DateTime.UtcNow,
            Type = TransactionType.Expense,
            UnitPrice = 15.00m,
            Quantity = 25,
            TransactionAmount = 375.00m,
            ProductVariantId = productVariant.Id,
            VendorId = vendor.Id
        };
        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetProductVariantPricingByVendorQuery.Args>(new(productVariant.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        var vendorPricing = result.Value!.VendorPricings.First();

        Assert.Equal("Test Vendor", vendorPricing.Vendor.Name);
        Assert.Equal("Test City", vendorPricing.Vendor.Location);
        Assert.NotNull(vendorPricing.Vendor.ContactPerson);
        Assert.Equal("Contact", vendorPricing.Vendor.ContactPerson.FirstName);
        Assert.Equal("Person", vendorPricing.Vendor.ContactPerson.LastName);
        Assert.Equal("contact@vendor.com", vendorPricing.Vendor.ContactPerson.Email);
    }
}

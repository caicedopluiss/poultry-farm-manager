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

public class GetProductVariantTransactionsQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetProductVariantTransactionsQuery.Args, GetProductVariantTransactionsQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetProductVariantTransactionsQuery.Args, GetProductVariantTransactionsQuery.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetProductVariantTransactionsQuery_ShouldReturnTransactions_WhenProductVariantHasTransactions()
    {
        // Arrange - Create product variant with multiple transactions
        var person = new Person { FirstName = "John", LastName = "Supplier" };
        dbContext.Persons.Add(person);

        var vendor1 = new Vendor { Name = "Vendor 1", ContactPersonId = person.Id };
        var vendor2 = new Vendor { Name = "Vendor 2", ContactPersonId = person.Id };
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
            Stock = 2500m,
            Quantity = 25
        };
        dbContext.ProductVariants.Add(productVariant);

        var transaction1 = new Transaction
        {
            Title = "Purchase 1",
            Date = DateTime.UtcNow.AddDays(-5),
            Type = TransactionType.Expense,
            UnitPrice = 15.00m,
            Quantity = 25,
            TransactionAmount = 375.00m,
            ProductVariantId = productVariant.Id,
            VendorId = vendor1.Id
        };

        var transaction2 = new Transaction
        {
            Title = "Purchase 2",
            Date = DateTime.UtcNow.AddDays(-2),
            Type = TransactionType.Expense,
            UnitPrice = 16.50m,
            Quantity = 30,
            TransactionAmount = 495.00m,
            ProductVariantId = productVariant.Id,
            VendorId = vendor2.Id
        };

        var transaction3 = new Transaction
        {
            Title = "Purchase 3",
            Date = DateTime.UtcNow,
            Type = TransactionType.Expense,
            UnitPrice = 14.75m,
            Quantity = 20,
            TransactionAmount = 295.00m,
            ProductVariantId = productVariant.Id,
            VendorId = vendor1.Id
        };

        dbContext.Transactions.AddRange(transaction1, transaction2, transaction3);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetProductVariantTransactionsQuery.Args>(new(productVariant.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value!.Transactions.Count());

        var transactions = result.Value.Transactions.ToList();

        // Should be ordered by date descending (newest first)
        Assert.Equal("Purchase 3", transactions[0].Title);
        Assert.Equal(14.75m, transactions[0].UnitPrice);
        Assert.Equal("Purchase 2", transactions[1].Title);
        Assert.Equal(16.50m, transactions[1].UnitPrice);
        Assert.Equal("Purchase 1", transactions[2].Title);
        Assert.Equal(15.00m, transactions[2].UnitPrice);
    }

    [Fact]
    public async Task GetProductVariantTransactionsQuery_ShouldReturnEmpty_WhenProductVariantHasNoTransactions()
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
            Stock = 2500m,
            Quantity = 25
        };
        dbContext.ProductVariants.Add(productVariant);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetProductVariantTransactionsQuery.Args>(new(productVariant.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.Transactions);
    }

    [Fact]
    public async Task GetProductVariantTransactionsQuery_ShouldFail_WithEmptyProductVariantId()
    {
        // Arrange
        var request = new AppRequest<GetProductVariantTransactionsQuery.Args>(new(Guid.Empty));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "productVariantId");
    }

    [Fact]
    public async Task GetProductVariantTransactionsQuery_ShouldIncludeVendorDetails()
    {
        // Arrange
        var person = new Person { FirstName = "Jane", LastName = "Smith", Email = "jane@supplier.com" };
        dbContext.Persons.Add(person);

        var vendor = new Vendor { Name = "Premium Supplier", ContactPersonId = person.Id };
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
            Name = "50kg Bag",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 5000m,
            Quantity = 50
        };
        dbContext.ProductVariants.Add(productVariant);

        var transaction = new Transaction
        {
            Title = "Initial Purchase",
            Date = DateTime.UtcNow,
            Type = TransactionType.Expense,
            UnitPrice = 28.00m,
            Quantity = 50,
            TransactionAmount = 1400.00m,
            ProductVariantId = productVariant.Id,
            VendorId = vendor.Id
        };
        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetProductVariantTransactionsQuery.Args>(new(productVariant.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        var transactionDto = result.Value!.Transactions.First();
        Assert.NotNull(transactionDto.VendorId);
        Assert.Equal("Premium Supplier", transactionDto.VendorName);
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.Assets;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Queries.Assets;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetAssetTransactionsQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetAssetTransactionsQuery.Args, GetAssetTransactionsQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetAssetTransactionsQuery.Args, GetAssetTransactionsQuery.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAssetTransactionsQuery_ShouldReturnTransactions_ForValidAssetId()
    {
        // Arrange
        var vendor = await dbContext.CreateVendorAsync();
        var asset = await dbContext.CreateAssetAsync(vendorId: vendor.Id, unitPrice: 100m);

        var request = new AppRequest<GetAssetTransactionsQuery.Args>(new(asset.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value!.Transactions);

        var transaction = result.Value!.Transactions.First();
        Assert.Equal(asset.Id, transaction.AssetId);
        Assert.Equal(vendor.Id, transaction.VendorId);
        Assert.Equal("Expense", transaction.Type);
    }

    [Fact]
    public async Task GetAssetTransactionsQuery_ShouldReturnEmpty_WhenNoTransactionsExist()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var request = new AppRequest<GetAssetTransactionsQuery.Args>(new(assetId));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.Transactions);
    }

    [Fact]
    public async Task GetAssetTransactionsQuery_ShouldFail_WithEmptyAssetId()
    {
        // Arrange
        var request = new AppRequest<GetAssetTransactionsQuery.Args>(new(Guid.Empty));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.Item1 == "assetId");
    }

    [Fact]
    public async Task GetAssetTransactionsQuery_ShouldOrderTransactions_ByDateDescending()
    {
        // Arrange
        var vendor = await dbContext.CreateVendorAsync();
        var asset = await dbContext.CreateAssetAsync(vendorId: vendor.Id, unitPrice: 100m);

        // Create additional transactions with different dates
        var transaction1 = await dbContext.CreateTransactionAsync(
            assetId: asset.Id,
            vendorId: vendor.Id,
            date: DateTime.UtcNow.AddDays(-10)
        );
        var transaction2 = await dbContext.CreateTransactionAsync(
            assetId: asset.Id,
            vendorId: vendor.Id,
            date: DateTime.UtcNow.AddDays(-5)
        );

        var request = new AppRequest<GetAssetTransactionsQuery.Args>(new(asset.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Transactions.Count() >= 3);

        var transactions = result.Value!.Transactions.ToList();
        for (int i = 0; i < transactions.Count - 1; i++)
        {
            var current = DateTime.Parse(transactions[i].Date);
            var next = DateTime.Parse(transactions[i + 1].Date);
            Assert.True(current >= next, "Transactions should be ordered by date descending");
        }
    }
}

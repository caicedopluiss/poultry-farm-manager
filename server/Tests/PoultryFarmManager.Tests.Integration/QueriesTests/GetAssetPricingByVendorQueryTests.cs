using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.Assets;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetAssetPricingByVendorQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetAssetPricingByVendorQuery.Args, GetAssetPricingByVendorQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetAssetPricingByVendorQuery.Args, GetAssetPricingByVendorQuery.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAssetPricingByVendorQuery_ShouldReturnVendorPricing_ForValidAssetId()
    {
        // Arrange
        var vendor = await dbContext.CreateVendorAsync();
        var asset = await dbContext.CreateAssetAsync(vendorId: vendor.Id, unitPrice: 100m);

        var request = new AppRequest<GetAssetPricingByVendorQuery.Args>(new(asset.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value!.VendorPricings);

        var vendorPricing = result.Value!.VendorPricings.First();
        Assert.Equal(vendor.Id, vendorPricing.Vendor.Id);
        Assert.Equal(100m, vendorPricing.LastUnitPrice);
        Assert.Equal(1, vendorPricing.TotalPurchases);
    }

    [Fact]
    public async Task GetAssetPricingByVendorQuery_ShouldReturnEmpty_WhenNoTransactionsExist()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var request = new AppRequest<GetAssetPricingByVendorQuery.Args>(new(assetId));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.VendorPricings);
    }

    [Fact]
    public async Task GetAssetPricingByVendorQuery_ShouldFail_WithEmptyAssetId()
    {
        // Arrange
        var request = new AppRequest<GetAssetPricingByVendorQuery.Args>(new(Guid.Empty));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.Item1 == "assetId");
    }

    [Fact]
    public async Task GetAssetPricingByVendorQuery_ShouldGroupByVendor_AndReturnLastPrice()
    {
        // Arrange
        var vendor1 = await dbContext.CreateVendorAsync();
        var vendor2 = await dbContext.CreateVendorAsync();
        var asset = await dbContext.CreateAssetAsync(vendorId: vendor1.Id, unitPrice: 100m);

        // Create additional transactions with different vendors and prices
        // Make the second transaction MORE RECENT so it becomes the "last" price
        await dbContext.CreateTransactionAsync(
            assetId: asset.Id,
            vendorId: vendor1.Id,
            unitPrice: 120m,
            date: DateTime.UtcNow.AddDays(1) // More recent than the initial transaction
        );
        await dbContext.CreateTransactionAsync(
            assetId: asset.Id,
            vendorId: vendor2.Id,
            unitPrice: 95m,
            date: DateTime.UtcNow.AddDays(2) // Even more recent
        );

        var request = new AppRequest<GetAssetPricingByVendorQuery.Args>(new(asset.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.VendorPricings.Count());

        var vendor1Pricing = result.Value!.VendorPricings.First(vp => vp.Vendor.Id == vendor1.Id);
        Assert.Equal(120m, vendor1Pricing.LastUnitPrice);
        Assert.Equal(2, vendor1Pricing.TotalPurchases);

        var vendor2Pricing = result.Value!.VendorPricings.First(vp => vp.Vendor.Id == vendor2.Id);
        Assert.Equal(95m, vendor2Pricing.LastUnitPrice);
        Assert.Equal(1, vendor2Pricing.TotalPurchases);
    }

    [Fact]
    public async Task GetAssetPricingByVendorQuery_ShouldOrderByLastPurchaseDate_Descending()
    {
        // Arrange
        var vendor1 = await dbContext.CreateVendorAsync();
        var vendor2 = await dbContext.CreateVendorAsync();
        var asset = await dbContext.CreateAssetAsync(vendorId: vendor1.Id, unitPrice: 100m); // Creates transaction at UtcNow

        // Create vendor2 transaction MORE RECENT than vendor1's initial transaction
        await dbContext.CreateTransactionAsync(
            assetId: asset.Id,
            vendorId: vendor2.Id,
            unitPrice: 95m,
            date: DateTime.UtcNow.AddDays(1) // Make it more recent
        );

        var request = new AppRequest<GetAssetPricingByVendorQuery.Args>(new(asset.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.VendorPricings.Count());

        var vendorPricings = result.Value!.VendorPricings.ToList();
        Assert.Equal(vendor2.Id, vendorPricings[0].Vendor.Id); // Most recent
        Assert.Equal(vendor1.Id, vendorPricings[1].Vendor.Id); // Older
    }
}

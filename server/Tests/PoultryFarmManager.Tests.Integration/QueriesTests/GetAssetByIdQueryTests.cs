using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.Assets;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetAssetByIdQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetAssetByIdQuery.Args, GetAssetByIdQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetAssetByIdQuery.Args, GetAssetByIdQuery.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAssetByIdQuery_ShouldReturnAsset_WhenExists()
    {
        // Arrange - Create an asset
        var asset = new Asset
        {
            Name = "Test Tractor",
            Description = "Heavy duty tractor",
            Notes = "Requires annual maintenance",
            States = [
                new AssetState
                {
                    Status = AssetStatus.Available,
                    Quantity = 1,
                    Location = "Warehouse A"
                }
            ]
        };
        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetAssetByIdQuery.Args>(new(asset.Id));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.Asset);
        Assert.Equal(asset.Id, result.Value!.Asset.Id);
        Assert.Equal("Test Tractor", result.Value!.Asset.Name);
        Assert.Equal("Heavy duty tractor", result.Value!.Asset.Description);
    }

    [Fact]
    public async Task GetAssetByIdQuery_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var request = new AppRequest<GetAssetByIdQuery.Args>(new(Guid.NewGuid()));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value!.Asset);
    }
}

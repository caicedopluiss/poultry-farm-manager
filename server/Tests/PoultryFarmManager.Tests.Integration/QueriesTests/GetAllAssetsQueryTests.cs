using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.Assets;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetAllAssetsQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetAllAssetsQuery.Args, GetAllAssetsQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetAllAssetsQuery.Args, GetAllAssetsQuery.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAllAssetsQuery_ShouldReturnAllAssets()
    {
        // Arrange - Create multiple assets
        var assets = new[]
        {
            new Asset
            {
                Name = "Tractor",
                States = [
                    new AssetState
                    {
                        Status = AssetStatus.Available,
                        Quantity = 1,
                        Location = "Warehouse A"
                    }
                ]
            },
            new Asset
            {
                Name = "Incubator",
                States = [
                    new AssetState
                    {
                        Status = AssetStatus.Available,
                        Quantity = 1,
                        Location = "Shed B"
                    }
                ]
            },
            new Asset
            {
                Name = "Feeder System"
            }
        };
        dbContext.Assets.AddRange(assets);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetAllAssetsQuery.Args>(new());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value!.Assets.Count());
        Assert.Contains(result.Value!.Assets, a => a.Name == "Tractor");
        Assert.Contains(result.Value!.Assets, a => a.Name == "Incubator");
        Assert.Contains(result.Value!.Assets, a => a.Name == "Feeder System");
    }

    [Fact]
    public async Task GetAllAssetsQuery_ShouldReturnEmptyList_WhenNoAssets()
    {
        // Arrange
        var request = new AppRequest<GetAllAssetsQuery.Args>(new());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.Assets);
    }
}

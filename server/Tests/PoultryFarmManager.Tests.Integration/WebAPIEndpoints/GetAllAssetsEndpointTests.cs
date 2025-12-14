using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;
using PoultryFarmManager.WebAPI.Endpoints.v1.Assets;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetAllAssetsEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_Assets_ShouldReturnAllAssets()
    {
        // Arrange - Create test assets
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
                Name = "Incubator"
            }
        };
        dbContext.Assets.AddRange(assets);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await fixture.Client.GetAsync("/api/v1/assets");
        var responseBody = await response.Content.ReadFromJsonAsync<GetAllAssetsEndpoint.GetAllAssetsResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Assets);
        Assert.Equal(2, responseBody.Assets.Count());
    }

    [Fact]
    public async Task GET_Assets_ShouldReturnEmptyList_WhenNoAssets()
    {
        // Act
        var response = await fixture.Client.GetAsync("/api/v1/assets");
        var responseBody = await response.Content.ReadFromJsonAsync<GetAllAssetsEndpoint.GetAllAssetsResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Assets);
        Assert.Empty(responseBody.Assets);
    }
}

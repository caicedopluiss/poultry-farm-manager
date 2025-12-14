using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;
using PoultryFarmManager.WebAPI.Endpoints.v1.Assets;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetAssetByIdEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_AssetById_ShouldReturnAsset()
    {
        // Arrange - Create test asset
        var asset = new Asset
        {
            Name = "Test Incubator",
            Description = "Test description",
            States = [
                new AssetState
                {
                    Status = AssetStatus.Available,
                    Quantity = 5,
                    Location = "Warehouse A"
                }
            ]
        };
        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/assets/{asset.Id}");
        var responseBody = await response.Content.ReadFromJsonAsync<GetAssetByIdEndpoint.GetAssetByIdResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Asset);
        Assert.Equal(asset.Id, responseBody.Asset.Id);
        Assert.Equal("Test Incubator", responseBody.Asset.Name);
    }

    [Fact]
    public async Task GET_AssetById_NonExistentId_ShouldReturnNotFound()
    {
        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/assets/{Guid.NewGuid()}");

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

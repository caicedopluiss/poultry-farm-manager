using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;
using PoultryFarmManager.WebAPI.Endpoints.v1.Assets;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class UpdateAssetEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task PUT_Asset_ValidRequest_ShouldReturnOk()
    {
        // Arrange - Create test asset
        var asset = new Asset
        {
            Name = "Old Name",
            States = [
                new AssetState
                {
                    Status = AssetStatus.Available,
                    Quantity = 1,
                    Location = "Old Location"
                }
            ]
        };
        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync();

        var updateAsset = new
        {
            Name = "Updated Name",
            Description = "Updated description",
            Location = "New Location",
            Notes = "Updated notes"
        };
        var body = new { updateAsset };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/assets/{asset.Id}", body);
        var responseBody = await response.Content.ReadFromJsonAsync<UpdateAssetEndpoint.UpdateAssetResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Asset);
        Assert.Equal(asset.Id, responseBody.Asset.Id);
        Assert.Equal("Updated Name", responseBody.Asset.Name);
    }

    [Fact]
    public async Task PUT_Asset_NonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var updateAsset = new
        {
            Name = "Updated Name",
            Description = "Updated description",
            Location = "Location",
            Notes = ""
        };
        var body = new { updateAsset };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/assets/{Guid.NewGuid()}", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

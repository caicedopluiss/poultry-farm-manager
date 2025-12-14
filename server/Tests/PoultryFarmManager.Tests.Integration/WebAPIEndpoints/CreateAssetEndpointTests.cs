using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.WebAPI.Endpoints.v1.Assets;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class CreateAssetEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task POST_Assets_ValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var newAsset = new NewAssetDto
        {
            Name = "Test Incubator from API",
            Description = "Automatic incubator for testing",
            InitialQuantity = 10,
            Notes = "API test asset"
        };
        var body = new { newAsset };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/assets", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreateAssetEndpoint.CreateAssetResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Asset);
        Assert.Equal("Test Incubator from API", responseBody.Asset.Name);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"/api/v1/assets/{responseBody.Asset.Id}", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task POST_Assets_InvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange - Empty name
        var newAsset = new
        {
            Name = "",
            Description = "",
            AcquisitionClientDateIsoString = DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes = ""
        };
        var body = new { newAsset };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/assets", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.WebAPI.Endpoints.v1.FeedingTables;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class UpdateFeedingTableEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task PATCH_FeedingTable_ValidRequest_ShouldReturnOkWithUpdatedFeedingTable()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync(name: "Original Name");
        var updates = new UpdateFeedingTableDto(Name: "Renamed Table", Description: null, DayEntries: null);
        var body = new { updates };

        // Act
        var response = await fixture.Client.PatchAsJsonAsync($"/api/v1/feeding-tables/{feedingTable.Id}", body);
        var responseBody = await response.Content.ReadFromJsonAsync<UpdateFeedingTableEndpoint.UpdateFeedingTableResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Equal("Renamed Table", responseBody.FeedingTable.Name);
    }

    [Fact]
    public async Task PATCH_FeedingTable_NonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var updates = new UpdateFeedingTableDto(Name: "Some Name", Description: null, DayEntries: null);
        var body = new { updates };

        // Act
        var response = await fixture.Client.PatchAsJsonAsync($"/api/v1/feeding-tables/{Guid.NewGuid()}", body);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PATCH_FeedingTable_EmptyDayEntries_ShouldReturnBadRequest()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var updates = new UpdateFeedingTableDto(Name: null, Description: null, DayEntries: []);
        var body = new { updates };

        // Act
        var response = await fixture.Client.PatchAsJsonAsync($"/api/v1/feeding-tables/{feedingTable.Id}", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

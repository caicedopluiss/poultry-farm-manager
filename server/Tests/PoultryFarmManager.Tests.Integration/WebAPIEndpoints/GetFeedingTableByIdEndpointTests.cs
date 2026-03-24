using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.WebAPI.Endpoints.v1.FeedingTables;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetFeedingTableByIdEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_FeedingTableById_ShouldReturnOkAndFeedingTable_WhenFound()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync(name: "Table To Get", dayCount: 2);

        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/feeding-tables/{feedingTable.Id}");
        var responseBody = await response.Content.ReadFromJsonAsync<GetFeedingTableByIdEndpoint.GetFeedingTableByIdResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.FeedingTable);
        Assert.Equal(feedingTable.Id, responseBody.FeedingTable.Id);
        Assert.Equal("Table To Get", responseBody.FeedingTable.Name);
        Assert.Equal(2, responseBody.FeedingTable.DayEntries.Count);
    }

    [Fact]
    public async Task GET_FeedingTableById_ShouldReturnNotFound_WhenFeedingTableDoesNotExist()
    {
        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/feeding-tables/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GET_FeedingTableById_ShouldReturnBadRequest_WhenIdIsEmpty()
    {
        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/feeding-tables/{Guid.Empty}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

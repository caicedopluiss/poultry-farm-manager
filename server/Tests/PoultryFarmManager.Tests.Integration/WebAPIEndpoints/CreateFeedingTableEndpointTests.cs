using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.WebAPI.Endpoints.v1.FeedingTables;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class CreateFeedingTableEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task POST_FeedingTables_ValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var newFeedingTable = new NewFeedingTableDto(
            Name: "Broiler Table",
            Description: "Standard broiler feeding schedule",
            DayEntries:
            [
                new NewFeedingTableDayEntryDto(1, "PreInicio", 50m, "Gram", null, null),
                new NewFeedingTableDayEntryDto(2, "Inicio", 100m, "Gram", null, null),
            ]);
        var body = new { newFeedingTable };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/feeding-tables", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreateFeedingTableEndpoint.CreateFeedingTableResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.FeedingTable);
        Assert.NotEqual(Guid.Empty, responseBody.FeedingTable.Id);
        Assert.Equal("Broiler Table", responseBody.FeedingTable.Name);
        Assert.Equal(2, responseBody.FeedingTable.DayEntries.Count);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"/api/v1/feeding-tables/{responseBody.FeedingTable.Id}", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task POST_FeedingTables_InvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange — empty name
        var newFeedingTable = new NewFeedingTableDto(
            Name: "",
            Description: null,
            DayEntries: [new NewFeedingTableDayEntryDto(1, "PreInicio", 50m, "Gram", null, null)]);
        var body = new { newFeedingTable };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/feeding-tables", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

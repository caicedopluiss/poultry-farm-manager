using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.WebAPI.Endpoints.v1.FeedingTables;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetFeedingTablesListEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_FeedingTables_ShouldReturnOkWithEmptyList_WhenNoFeedingTablesExist()
    {
        // Act
        var response = await fixture.Client.GetAsync("/api/v1/feeding-tables");
        var responseBody = await response.Content.ReadFromJsonAsync<GetFeedingTablesListEndpoint.GetFeedingTablesListResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Empty(responseBody.FeedingTables);
    }

    [Fact]
    public async Task GET_FeedingTables_ShouldReturnOkWithAllFeedingTables()
    {
        // Arrange
        await dbContext.CreateFeedingTableAsync(name: "Table A");
        await dbContext.CreateFeedingTableAsync(name: "Table B");

        // Act
        var response = await fixture.Client.GetAsync("/api/v1/feeding-tables");
        var responseBody = await response.Content.ReadFromJsonAsync<GetFeedingTablesListEndpoint.GetFeedingTablesListResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Equal(2, responseBody.FeedingTables.Count());
    }
}

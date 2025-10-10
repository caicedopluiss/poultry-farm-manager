using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.WebAPI.Endpoints.v1.Batches;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetBatchesListEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_Batches_ShouldReturnOkAndEmptyList_WhenNoBatchesExist()
    {
        // Act - Real HTTP GET request to your API
        var response = await fixture.Client.GetAsync("/api/v1/batches");
        var responseBody = await response.Content.ReadFromJsonAsync<GetBatchesListEndpoint.GetBatchesListResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Batches);
        Assert.Empty(responseBody.Batches);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task GET_Batches_ShouldReturnOkAndBatchesList_WhenBatchesExist(int batchCount)
    {
        // Arrange - Add batches to the database
        for (int i = 0; i < batchCount; i++)
        {
            var batch = fixture.CreateRandomEntity<Core.Models.Batch>();
            dbContext.Batches.Add(batch);
        }
        await dbContext.SaveChangesAsync();

        // Act - Real HTTP GET request to your API
        var response = await fixture.Client.GetAsync("/api/v1/batches");
        var responseBody = await response.Content.ReadFromJsonAsync<GetBatchesListEndpoint.GetBatchesListResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Batches);
        Assert.Equal(batchCount, responseBody.Batches.Count());
    }

}

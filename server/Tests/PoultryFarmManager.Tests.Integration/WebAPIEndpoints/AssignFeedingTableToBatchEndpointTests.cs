using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.WebAPI.Endpoints.v1.Batches;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class AssignFeedingTableToBatchEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task PATCH_BatchFeedingTable_ValidRequest_ShouldReturnOkWithUpdatedBatch()
    {
        // Arrange
        var batch = fixture.CreateRandomEntity<Batch>();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var body = new { feedingTableId = feedingTable.Id };

        // Act
        var response = await fixture.Client.PatchAsJsonAsync($"/api/v1/batches/{batch.Id}/feeding-table", body);
        var responseBody = await response.Content.ReadFromJsonAsync<AssignFeedingTableToBatchEndpoint.AssignFeedingTableToBatchResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Batch);
        Assert.Equal(batch.Id, responseBody.Batch.Id);
    }

    [Fact]
    public async Task PATCH_BatchFeedingTable_NullFeedingTableId_ShouldUnassignAndReturnOk()
    {
        // Arrange — batch already has a feeding table
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var batch = fixture.CreateRandomEntity<Batch>();
        batch.FeedingTableId = feedingTable.Id;
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var body = new { feedingTableId = (Guid?)null };

        // Act
        var response = await fixture.Client.PatchAsJsonAsync($"/api/v1/batches/{batch.Id}/feeding-table", body);
        var responseBody = await response.Content.ReadFromJsonAsync<AssignFeedingTableToBatchEndpoint.AssignFeedingTableToBatchResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Null(responseBody.Batch.FeedingTable);
    }

    [Fact]
    public async Task PATCH_BatchFeedingTable_NonExistentBatch_ShouldReturnNotFound()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var body = new { feedingTableId = feedingTable.Id };

        // Act
        var response = await fixture.Client.PatchAsJsonAsync($"/api/v1/batches/{Guid.NewGuid()}/feeding-table", body);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

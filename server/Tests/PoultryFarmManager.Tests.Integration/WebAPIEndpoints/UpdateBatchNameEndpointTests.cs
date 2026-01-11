using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.WebAPI.Endpoints.v1.Batches;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class UpdateBatchNameEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task PUT_BatchName_ValidRequest_ShouldReturnOk()
    {
        // Arrange - Create test batch
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch.Name = "Original Name";
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var body = new { name = "Updated Batch Name" };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{batch.Id}/name", body);
        var responseBody = await response.Content.ReadFromJsonAsync<UpdateBatchNameEndpoint.UpdateBatchNameResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Batch);
        Assert.Equal(batch.Id, responseBody.Batch.Id);
        Assert.Equal("Updated Batch Name", responseBody.Batch.Name);

        // Verify in database - clear change tracker to get fresh data
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.Equal("Updated Batch Name", updatedBatch.Name);
    }

    [Fact]
    public async Task PUT_BatchName_NonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var body = new { name = "Some Name" };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{Guid.NewGuid()}/name", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PUT_BatchName_EmptyName_ShouldReturnBadRequest()
    {
        // Arrange - Create test batch
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var body = new { name = "" };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{batch.Id}/name", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PUT_BatchName_NameTooLong_ShouldReturnBadRequest()
    {
        // Arrange - Create test batch
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var body = new { name = new string('A', 101) };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{batch.Id}/name", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PUT_BatchName_DuplicateName_ShouldReturnBadRequest()
    {
        // Arrange - Create two batches
        var batch1 = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch1.Name = "Existing Batch Name";
        var batch2 = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch2.Name = "Different Name";
        dbContext.Batches.AddRange(batch1, batch2);
        await dbContext.SaveChangesAsync();

        // Try to update batch2 with batch1's name
        var body = new { name = "Existing Batch Name" };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{batch2.Id}/name", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PUT_BatchName_SameName_ShouldReturnOk()
    {
        // Arrange - Create test batch
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch.Name = "Current Name";
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        // Update with same name (should be allowed)
        var body = new { name = "Current Name" };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{batch.Id}/name", body);
        var responseBody = await response.Content.ReadFromJsonAsync<UpdateBatchNameEndpoint.UpdateBatchNameResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Equal("Current Name", responseBody.Batch.Name);
    }

    [Fact]
    public async Task PUT_BatchName_WhitespaceName_ShouldReturnBadRequest()
    {
        // Arrange - Create test batch
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var body = new { name = "   " };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{batch.Id}/name", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

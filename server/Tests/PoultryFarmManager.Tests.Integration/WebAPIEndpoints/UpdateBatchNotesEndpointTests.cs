using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.WebAPI.Endpoints.v1.Batches;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class UpdateBatchNotesEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task PUT_BatchNotes_ValidRequest_ShouldReturnOk()
    {
        // Arrange - Create test batch
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch.Notes = null;
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var body = new { notes = "These are the batch notes" };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{batch.Id}/notes", body);
        var responseBody = await response.Content.ReadFromJsonAsync<UpdateBatchNotesEndpoint.UpdateBatchNotesResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.True(responseBody.Success);

        // Verify in database - clear change tracker to get fresh data
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.Equal("These are the batch notes", updatedBatch.Notes);
    }

    [Fact]
    public async Task PUT_BatchNotes_UpdateExistingNotes_ShouldReturnOk()
    {
        // Arrange - Create test batch with existing notes
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch.Notes = "Original notes";
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var body = new { notes = "Updated notes" };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{batch.Id}/notes", body);
        var responseBody = await response.Content.ReadFromJsonAsync<UpdateBatchNotesEndpoint.UpdateBatchNotesResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.True(responseBody.Success);

        // Verify in database
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.Equal("Updated notes", updatedBatch.Notes);
    }

    [Fact]
    public async Task PUT_BatchNotes_NullNotes_ShouldClearNotes()
    {
        // Arrange - Create test batch with existing notes
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch.Notes = "Existing notes";
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var body = new { notes = (string?)null };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{batch.Id}/notes", body);
        var responseBody = await response.Content.ReadFromJsonAsync<UpdateBatchNotesEndpoint.UpdateBatchNotesResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.True(responseBody.Success);

        // Verify in database
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.Null(updatedBatch.Notes);
    }

    [Fact]
    public async Task PUT_BatchNotes_NonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var body = new { notes = "Some notes" };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{Guid.NewGuid()}/notes", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PUT_BatchNotes_NotesTooLong_ShouldReturnBadRequest()
    {
        // Arrange - Create test batch
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var body = new { notes = new string('A', 501) };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{batch.Id}/notes", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PUT_BatchNotes_MaxLengthNotes_ShouldReturnOk()
    {
        // Arrange - Create test batch
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var maxLengthNotes = new string('A', 500);
        var body = new { notes = maxLengthNotes };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{batch.Id}/notes", body);
        var responseBody = await response.Content.ReadFromJsonAsync<UpdateBatchNotesEndpoint.UpdateBatchNotesResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.True(responseBody.Success);

        // Verify in database
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.NotNull(updatedBatch.Notes);
        Assert.Equal(maxLengthNotes, updatedBatch.Notes);
        Assert.Equal(500, updatedBatch.Notes.Length);
    }

    [Fact]
    public async Task PUT_BatchNotes_MultilineNotes_ShouldReturnOk()
    {
        // Arrange - Create test batch
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var multilineNotes = "Line 1\nLine 2\nLine 3";
        var body = new { notes = multilineNotes };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{batch.Id}/notes", body);
        var responseBody = await response.Content.ReadFromJsonAsync<UpdateBatchNotesEndpoint.UpdateBatchNotesResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.True(responseBody.Success);

        // Verify in database
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.Equal(multilineNotes, updatedBatch.Notes);
    }

    [Fact]
    public async Task PUT_BatchNotes_EmptyBatchId_ShouldReturnBadRequest()
    {
        // Arrange
        var body = new { notes = "Some notes" };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/batches/{Guid.Empty}/notes", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

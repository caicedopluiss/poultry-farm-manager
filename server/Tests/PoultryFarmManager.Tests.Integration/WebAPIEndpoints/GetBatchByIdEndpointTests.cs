using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.WebAPI;
using PoultryFarmManager.WebAPI.Endpoints.v1.Batches;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetBatchByIdEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_BatchById_ShouldReturnNotFound_WhenBatchDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act - Real HTTP GET request to your API
        var response = await fixture.Client.GetAsync($"/api/v1/batches/{nonExistentId}");
        var responseBody = await response.Content.ReadFromJsonAsync<ErrorResponse>();


        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, responseBody?.StatusCode);
    }

    [Fact]
    public async Task GET_BatchById_ShouldReturnOkAndBatch_WhenBatchExists()
    {
        // Arrange - Add a batch to the database
        var batch = fixture.CreateRandomEntity<Batch>();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        // Act - Real HTTP GET request to your API
        var response = await fixture.Client.GetAsync($"/api/v1/batches/{batch.Id}");
        var responseBody = await response.Content.ReadFromJsonAsync<GetBatchByIdEndpoint.GetBatchByIdResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Batch);
    }

    [Fact]
    public async Task GET_BatchById_ShouldReturnBadRequest_WhenIdIsEmpty()
    {
        // Act - Real HTTP GET request with empty GUID
        var response = await fixture.Client.GetAsync($"/api/v1/batches/{Guid.Empty}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

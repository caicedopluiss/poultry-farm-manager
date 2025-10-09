using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.WebAPI.Endpoints.v1.Batches;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class CreateBatchEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();


    [Fact]
    public async Task POST_Batches_ValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var newBatch = new NewBatchDto
        (
            Name: "Test Batch from API",
            Breed: "Leghorn",
            StartClientDateIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            MaleCount: 100,
            FemaleCount: 150,
            UnsexedCount: 50,
            Shed: "Shed API-1"
        );
        var body = new { newBatch };

        // Act - Real HTTP POST request to your API
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/batches", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreateBatchEndpoint.CreateBatchEndpointResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Batch);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"/api/v1/batches/{responseBody.Batch.Id}", response.Headers.Location.ToString());
    }
}


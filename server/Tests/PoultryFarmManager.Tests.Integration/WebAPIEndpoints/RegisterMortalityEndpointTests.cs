using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.WebAPI.Endpoints.v1.Batches;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class RegisterMortalityEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task POST_Mortality_ValidRequest_ShouldReturnCreatedWithLocationHeader()
    {
        // Arrange - Create a batch first
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Mortality API",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 50,
            InitialPopulation = 250,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-1"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var mortalityRegistration = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 10,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Unsexed",
            Notes: "Disease outbreak"
        );
        var body = new RegisterMortalityEndpoint.RegisterMortalityRequestBody(mortalityRegistration);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/mortality", body);
        var responseBody = await response.Content.ReadFromJsonAsync<RegisterMortalityEndpoint.RegisterMortalityResponseBody>();

        // Assert - HTTP-specific concerns
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"/api/v1/batches/{batch.Id}/mortality/", response.Headers.Location.ToString());

        // Response body structure
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.MortalityRegistration);
        Assert.Equal(batch.Id, responseBody.MortalityRegistration.BatchId);
        Assert.Equal(10, responseBody.MortalityRegistration.NumberOfDeaths);
        Assert.Equal("Unsexed", responseBody.MortalityRegistration.Sex);
        Assert.Equal("Disease outbreak", responseBody.MortalityRegistration.Notes);
    }

    [Fact]
    public async Task POST_Mortality_NonExistentBatch_ShouldReturn404()
    {
        // Arrange
        var nonExistentBatchId = Guid.NewGuid();
        var mortalityRegistration = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 10,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Unsexed",
            Notes: null
        );
        var body = new RegisterMortalityEndpoint.RegisterMortalityRequestBody(mortalityRegistration);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{nonExistentBatchId}/mortality", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task POST_Mortality_ValidationError_ShouldReturn400()
    {
        // Arrange - Test that validation errors return 400 (any validation error)
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Validation Error API",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 50,
            FemaleCount = 50,
            UnsexedCount = 20,
            InitialPopulation = 120,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-2"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var mortalityRegistration = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 0, // Invalid - triggers validation error
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Unsexed",
            Notes: null
        );
        var body = new RegisterMortalityEndpoint.RegisterMortalityRequestBody(mortalityRegistration);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/mortality", body);

        // Assert - Only test HTTP status code, not the specific validation logic
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

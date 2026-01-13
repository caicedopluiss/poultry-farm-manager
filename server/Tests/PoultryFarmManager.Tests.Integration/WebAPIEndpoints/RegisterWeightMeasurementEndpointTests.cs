using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.WebAPI.Endpoints.v1.Batches;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class RegisterWeightMeasurementEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task POST_WeightMeasurement_ValidRequest_ShouldReturnCreatedWithLocationHeader()
    {
        // Arrange - Create a batch first
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Weight Measurement API",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-1"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto
        (
            AverageWeight: 2.5m,
            SampleSize: 50,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Week 3 weight check"
        );
        var body = new RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementRequestBody(weightMeasurement);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/weight-measurements", body);
        var responseBody = await response.Content.ReadFromJsonAsync<RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementResponseBody>();

        // Assert - HTTP-specific concerns
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"/api/v1/batches/{batch.Id}/weight-measurements/", response.Headers.Location.ToString());

        // Response body structure
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.WeightMeasurement);
        Assert.Equal(batch.Id, responseBody.WeightMeasurement.BatchId);
        Assert.Equal(2.5m, responseBody.WeightMeasurement.AverageWeight);
        Assert.Equal(50, responseBody.WeightMeasurement.SampleSize);
        Assert.Equal("Kilogram", responseBody.WeightMeasurement.UnitOfMeasure);
        Assert.Equal("Week 3 weight check", responseBody.WeightMeasurement.Notes);
    }

    [Fact]
    public async Task POST_WeightMeasurement_WithGrams_ShouldReturnCreated()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Weight in Grams API",
            Breed = "Layer",
            StartDate = DateTime.UtcNow,
            MaleCount = 80,
            FemaleCount = 120,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-2"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto
        (
            AverageWeight: 1850m,
            SampleSize: 30,
            UnitOfMeasure: "Gram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Young layer weight"
        );
        var body = new RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementRequestBody(weightMeasurement);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/weight-measurements", body);
        var responseBody = await response.Content.ReadFromJsonAsync<RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Equal(1850m, responseBody.WeightMeasurement.AverageWeight);
        Assert.Equal("Gram", responseBody.WeightMeasurement.UnitOfMeasure);
        Assert.Equal(30, responseBody.WeightMeasurement.SampleSize);
    }

    [Fact]
    public async Task POST_WeightMeasurement_NonExistentBatch_ShouldReturn404()
    {
        // Arrange
        var nonExistentBatchId = Guid.NewGuid();
        var weightMeasurement = new NewWeightMeasurementDto
        (
            AverageWeight: 2.5m,
            SampleSize: 20,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var body = new RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementRequestBody(weightMeasurement);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{nonExistentBatchId}/weight-measurements", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task POST_WeightMeasurement_InvalidAverageWeight_ShouldReturn400()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Invalid Weight API",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-3"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto
        (
            AverageWeight: 0m, // Invalid - triggers validation error
            SampleSize: 20,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var body = new RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementRequestBody(weightMeasurement);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/weight-measurements", body);

        // Assert - Only test HTTP status code
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_WeightMeasurement_InvalidSampleSize_ShouldReturn400()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Invalid Sample Size API",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-4"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto
        (
            AverageWeight: 2.5m,
            SampleSize: 0, // Invalid - triggers validation error
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var body = new RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementRequestBody(weightMeasurement);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/weight-measurements", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_WeightMeasurement_InvalidUnitOfMeasure_ShouldReturn400()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Invalid Unit API",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-5"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto
        (
            AverageWeight: 2.5m,
            SampleSize: 20,
            UnitOfMeasure: "InvalidUnit", // Invalid - triggers validation error
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var body = new RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementRequestBody(weightMeasurement);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/weight-measurements", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_WeightMeasurement_InvalidDateFormat_ShouldReturn400()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Invalid Date API",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-6"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto
        (
            AverageWeight: 2.5m,
            SampleSize: 20,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: "invalid-date-format", // Invalid - triggers validation error
            Notes: null
        );
        var body = new RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementRequestBody(weightMeasurement);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/weight-measurements", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_WeightMeasurement_WithNullNotes_ShouldReturnCreated()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Null Notes API",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-7"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto
        (
            AverageWeight: 2.5m,
            SampleSize: 20,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var body = new RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementRequestBody(weightMeasurement);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/weight-measurements", body);
        var responseBody = await response.Content.ReadFromJsonAsync<RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Null(responseBody.WeightMeasurement.Notes);
    }

    [Fact]
    public async Task POST_WeightMeasurement_ForProcessedBatch_ShouldReturnBadRequest()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Processed Batch API",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow.AddDays(-60),
            MaleCount = 0,
            FemaleCount = 0,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = Core.Enums.BatchStatus.Processed,
            Shed = "Shed API-8"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto
        (
            AverageWeight: 2.5m,
            SampleSize: 20,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Attempting on processed batch"
        );
        var body = new RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementRequestBody(weightMeasurement);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/weight-measurements", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_WeightMeasurement_ForCanceledBatch_ShouldReturnBadRequest()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Canceled Batch API",
            Breed = "Layer",
            StartDate = DateTime.UtcNow.AddDays(-30),
            MaleCount = 50,
            FemaleCount = 50,
            UnsexedCount = 0,
            InitialPopulation = 100,
            Status = Core.Enums.BatchStatus.Canceled,
            Shed = "Shed API-9"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto
        (
            AverageWeight: 1.8m,
            SampleSize: 15,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var body = new RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementRequestBody(weightMeasurement);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/weight-measurements", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_WeightMeasurement_WithNonWeightUnit_ShouldReturnBadRequest()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Invalid Unit API",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-10"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto
        (
            AverageWeight: 2.5m,
            SampleSize: 20,
            UnitOfMeasure: "Liter", // Invalid: not a weight unit
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var body = new RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementRequestBody(weightMeasurement);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/weight-measurements", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_WeightMeasurement_WithUnitAsUnitOfMeasure_ShouldReturnBadRequest()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Unit UOM API",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-11"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto
        (
            AverageWeight: 2.5m,
            SampleSize: 20,
            UnitOfMeasure: "Unit", // Invalid: not a weight unit
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var body = new RegisterWeightMeasurementEndpoint.RegisterWeightMeasurementRequestBody(weightMeasurement);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/weight-measurements", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

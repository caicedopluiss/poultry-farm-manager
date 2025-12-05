using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.WebAPI.Endpoints.v1.Batches;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class SwitchBatchStatusEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task POST_Status_ValidRequest_ShouldReturnCreatedWithLocationHeader()
    {
        // Arrange - Create an active batch
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Status API",
            Breed = "Layer",
            StartDate = DateTime.UtcNow.AddDays(-30),
            MaleCount = 80,
            FemaleCount = 80,
            UnsexedCount = 10,
            InitialPopulation = 170,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-S1"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var statusSwitch = new NewStatusSwitchDto
        (
            NewStatus: "Processed",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Processing completed"
        );
        var body = new SwitchBatchStatusEndpoint.SwitchStatusRequestBody(statusSwitch);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/status", body);
        var responseBody = await response.Content.ReadFromJsonAsync<SwitchBatchStatusEndpoint.SwitchStatusResponseBody>();

        // Assert - HTTP-specific concerns
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"/api/v1/batches/{batch.Id}/status/", response.Headers.Location.ToString());

        // Response body structure
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.StatusSwitch);
        Assert.Equal(batch.Id, responseBody.StatusSwitch.BatchId);
        Assert.Equal("Processed", responseBody.StatusSwitch.NewStatus);
        Assert.Equal("Processing completed", responseBody.StatusSwitch.Notes);

        // Verify batch status was updated
        dbContext.Entry(batch).Reload();
        Assert.Equal(Core.Enums.BatchStatus.Processed, batch.Status);
    }

    [Fact]
    public async Task POST_Status_ActiveToForSale_ShouldSucceed()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Direct to ForSale API",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow.AddDays(-45),
            MaleCount = 90,
            FemaleCount = 90,
            UnsexedCount = 0,
            InitialPopulation = 180,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-S2"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var statusSwitch = new NewStatusSwitchDto
        (
            NewStatus: "ForSale",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Ready for market"
        );
        var body = new SwitchBatchStatusEndpoint.SwitchStatusRequestBody(statusSwitch);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/status", body);
        var responseBody = await response.Content.ReadFromJsonAsync<SwitchBatchStatusEndpoint.SwitchStatusResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("ForSale", responseBody!.StatusSwitch.NewStatus);
    }

    [Fact]
    public async Task POST_Status_ProcessedToForSale_ShouldSucceed()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Processed to ForSale API",
            Breed = "Mixed",
            StartDate = DateTime.UtcNow.AddDays(-60),
            MaleCount = 70,
            FemaleCount = 70,
            UnsexedCount = 0,
            InitialPopulation = 140,
            Status = Core.Enums.BatchStatus.Processed,
            Shed = "Shed API-S3"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var statusSwitch = new NewStatusSwitchDto
        (
            NewStatus: "ForSale",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var body = new SwitchBatchStatusEndpoint.SwitchStatusRequestBody(statusSwitch);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/status", body);
        var responseBody = await response.Content.ReadFromJsonAsync<SwitchBatchStatusEndpoint.SwitchStatusResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("ForSale", responseBody!.StatusSwitch.NewStatus);
    }

    [Fact]
    public async Task POST_Status_ForSaleToSold_ShouldSucceed()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch ForSale to Sold API",
            Breed = "Layer",
            StartDate = DateTime.UtcNow.AddDays(-75),
            MaleCount = 60,
            FemaleCount = 60,
            UnsexedCount = 0,
            InitialPopulation = 120,
            Status = Core.Enums.BatchStatus.ForSale,
            Shed = "Shed API-S4"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var statusSwitch = new NewStatusSwitchDto
        (
            NewStatus: "Sold",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Sold to buyer"
        );
        var body = new SwitchBatchStatusEndpoint.SwitchStatusRequestBody(statusSwitch);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/status", body);
        var responseBody = await response.Content.ReadFromJsonAsync<SwitchBatchStatusEndpoint.SwitchStatusResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Sold", responseBody!.StatusSwitch.NewStatus);
    }

    [Fact]
    public async Task POST_Status_NonExistentBatch_ShouldReturn404()
    {
        // Arrange
        var nonExistentBatchId = Guid.NewGuid();
        var statusSwitch = new NewStatusSwitchDto
        (
            NewStatus: "Processed",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var body = new SwitchBatchStatusEndpoint.SwitchStatusRequestBody(statusSwitch);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{nonExistentBatchId}/status", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task POST_Status_InvalidTransition_ShouldReturn400()
    {
        // Arrange - Try invalid transition (Processed -> Active)
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Invalid Transition API",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow.AddDays(-30),
            MaleCount = 50,
            FemaleCount = 50,
            UnsexedCount = 0,
            InitialPopulation = 100,
            Status = Core.Enums.BatchStatus.Processed,
            Shed = "Shed API-S5"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var statusSwitch = new NewStatusSwitchDto
        (
            NewStatus: "Active",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Trying to go back"
        );
        var body = new SwitchBatchStatusEndpoint.SwitchStatusRequestBody(statusSwitch);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/status", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_Status_InvalidStatusValue_ShouldReturn400()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Invalid Status Value API",
            Breed = "Layer",
            StartDate = DateTime.UtcNow.AddDays(-20),
            MaleCount = 40,
            FemaleCount = 40,
            UnsexedCount = 0,
            InitialPopulation = 80,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-S6"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var statusSwitch = new NewStatusSwitchDto
        (
            NewStatus: "InvalidStatus",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var body = new SwitchBatchStatusEndpoint.SwitchStatusRequestBody(statusSwitch);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/status", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_Status_MultipleStatusSwitches_ShouldCreateAllActivities()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Multiple Switches API",
            Breed = "Mixed",
            StartDate = DateTime.UtcNow.AddDays(-90),
            MaleCount = 35,
            FemaleCount = 35,
            UnsexedCount = 0,
            InitialPopulation = 70,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-S7"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        // Act - First switch: Active -> Processed
        var firstSwitch = new NewStatusSwitchDto("Processed", DateTime.UtcNow.ToString(Constants.DateTimeFormat), "First");
        var firstBody = new SwitchBatchStatusEndpoint.SwitchStatusRequestBody(firstSwitch);
        var firstResponse = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/status", firstBody);

        // Act - Second switch: Processed -> ForSale
        var secondSwitch = new NewStatusSwitchDto("ForSale", DateTime.UtcNow.ToString(Constants.DateTimeFormat), "Second");
        var secondBody = new SwitchBatchStatusEndpoint.SwitchStatusRequestBody(secondSwitch);
        var secondResponse = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/status", secondBody);

        // Get batch details to verify activities are returned
        var batchResponse = await fixture.Client.GetAsync($"/api/v1/batches/{batch.Id}");
        var batchBody = await batchResponse.Content.ReadFromJsonAsync<GetBatchByIdEndpoint.GetBatchByIdResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);

        // Verify activities are returned
        Assert.NotNull(batchBody);
        Assert.NotNull(batchBody.Activities);
        Assert.Equal(2, batchBody.Activities.Count());
    }

    [Fact]
    public async Task POST_Status_WithLongNotes_ShouldSucceed()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Long Notes API",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow.AddDays(-25),
            MaleCount = 45,
            FemaleCount = 45,
            UnsexedCount = 0,
            InitialPopulation = 90,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-S8"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var longNotes = new string('A', 450); // 450 characters (within 500 limit)
        var statusSwitch = new NewStatusSwitchDto
        (
            NewStatus: "Canceled",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: longNotes
        );
        var body = new SwitchBatchStatusEndpoint.SwitchStatusRequestBody(statusSwitch);

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/batches/{batch.Id}/status", body);
        var responseBody = await response.Content.ReadFromJsonAsync<SwitchBatchStatusEndpoint.SwitchStatusResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(longNotes, responseBody!.StatusSwitch.Notes);
    }
}

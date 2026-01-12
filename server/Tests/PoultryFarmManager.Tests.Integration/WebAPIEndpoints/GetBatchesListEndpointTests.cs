using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.BatchActivities;
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

    [Fact]
    public async Task GET_Batches_ShouldReturnBatchesWithNullFirstStatusChangeDate_WhenNoStatusSwitchActivitiesExist()
    {
        // Arrange - Add batches without status switch activities
        var batch1 = fixture.CreateRandomEntity<Core.Models.Batch>();
        var batch2 = fixture.CreateRandomEntity<Core.Models.Batch>();
        dbContext.Batches.Add(batch1);
        dbContext.Batches.Add(batch2);
        await dbContext.SaveChangesAsync();

        // Act - Real HTTP GET request to your API
        var response = await fixture.Client.GetAsync("/api/v1/batches");
        var responseBody = await response.Content.ReadFromJsonAsync<GetBatchesListEndpoint.GetBatchesListResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Batches);
        Assert.Equal(2, responseBody.Batches.Count());
        Assert.All(responseBody.Batches, b => Assert.Null(b.FirstStatusChangeDate));
    }

    [Fact]
    public async Task GET_Batches_ShouldReturnBatchesWithFirstStatusChangeDate_WhenStatusSwitchActivitiesExist()
    {
        // Arrange - Add batches with status switch activities
        var batch1 = fixture.CreateRandomEntity<Core.Models.Batch>();
        var batch2 = fixture.CreateRandomEntity<Core.Models.Batch>();
        dbContext.Batches.Add(batch1);
        dbContext.Batches.Add(batch2);
        await dbContext.SaveChangesAsync();

        // Add multiple status switches for batch1 (should return the earliest)
        var statusSwitch1_1 = new StatusSwitchBatchActivity
        {
            BatchId = batch1.Id,
            Date = System.DateTime.UtcNow.AddDays(-15),
            NewStatus = BatchStatus.Processed,
            Type = BatchActivityType.StatusSwitch
        };
        var statusSwitch1_2 = new StatusSwitchBatchActivity
        {
            BatchId = batch1.Id,
            Date = System.DateTime.UtcNow.AddDays(-8),
            NewStatus = BatchStatus.ForSale,
            Type = BatchActivityType.StatusSwitch
        };
        dbContext.StatusSwitchActivities.Add(statusSwitch1_1);
        dbContext.StatusSwitchActivities.Add(statusSwitch1_2);

        // Add single status switch for batch2
        var statusSwitch2 = new StatusSwitchBatchActivity
        {
            BatchId = batch2.Id,
            Date = System.DateTime.UtcNow.AddDays(-12),
            NewStatus = BatchStatus.Canceled,
            Type = BatchActivityType.StatusSwitch
        };
        dbContext.StatusSwitchActivities.Add(statusSwitch2);
        await dbContext.SaveChangesAsync();

        // Act - Real HTTP GET request to your API
        var response = await fixture.Client.GetAsync("/api/v1/batches");
        var responseBody = await response.Content.ReadFromJsonAsync<GetBatchesListEndpoint.GetBatchesListResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Batches);
        Assert.Equal(2, responseBody.Batches.Count());

        var batch1Dto = responseBody.Batches.First(b => b.Id == batch1.Id);
        var batch2Dto = responseBody.Batches.First(b => b.Id == batch2.Id);

        // Batch1 should have the earliest status switch date
        Assert.NotNull(batch1Dto.FirstStatusChangeDate);
        Assert.Equal(statusSwitch1_1.Date.ToString(Application.Constants.DateTimeFormat), batch1Dto.FirstStatusChangeDate);

        // Batch2 should have its status switch date
        Assert.NotNull(batch2Dto.FirstStatusChangeDate);
        Assert.Equal(statusSwitch2.Date.ToString(Application.Constants.DateTimeFormat), batch2Dto.FirstStatusChangeDate);
    }

    [Fact]
    public async Task GET_Batches_ShouldReturnMixedBatches_WithAndWithoutStatusSwitchActivities()
    {
        // Arrange - Add batches with and without status switch activities
        var batchWithActivity = fixture.CreateRandomEntity<Core.Models.Batch>();
        var batchWithoutActivity = fixture.CreateRandomEntity<Core.Models.Batch>();
        dbContext.Batches.Add(batchWithActivity);
        dbContext.Batches.Add(batchWithoutActivity);
        await dbContext.SaveChangesAsync();

        // Add status switch for only one batch
        var statusSwitch = new StatusSwitchBatchActivity
        {
            BatchId = batchWithActivity.Id,
            Date = System.DateTime.UtcNow.AddDays(-6),
            NewStatus = BatchStatus.Processed,
            Type = BatchActivityType.StatusSwitch
        };
        dbContext.StatusSwitchActivities.Add(statusSwitch);
        await dbContext.SaveChangesAsync();

        // Act - Real HTTP GET request to your API
        var response = await fixture.Client.GetAsync("/api/v1/batches");
        var responseBody = await response.Content.ReadFromJsonAsync<GetBatchesListEndpoint.GetBatchesListResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Batches);
        Assert.Equal(2, responseBody.Batches.Count());

        var batchWithActivityDto = responseBody.Batches.First(b => b.Id == batchWithActivity.Id);
        var batchWithoutActivityDto = responseBody.Batches.First(b => b.Id == batchWithoutActivity.Id);

        // Batch with activity should have FirstStatusChangeDate
        Assert.NotNull(batchWithActivityDto.FirstStatusChangeDate);
        Assert.Equal(statusSwitch.Date.ToString(Application.Constants.DateTimeFormat), batchWithActivityDto.FirstStatusChangeDate);

        // Batch without activity should have null FirstStatusChangeDate
        Assert.Null(batchWithoutActivityDto.FirstStatusChangeDate);
    }

}

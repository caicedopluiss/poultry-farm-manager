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

    [Fact]
    public async Task GET_Batches_ShouldReturnBatchesSortedByNameDescending_WhenSortByNameAndSortOrderDesc()
    {
        // Arrange - Add batches with specific names
        var batch1 = fixture.CreateRandomEntity<Core.Models.Batch>();
        batch1.Name = "Alpha";
        var batch2 = fixture.CreateRandomEntity<Core.Models.Batch>();
        batch2.Name = "Bravo";
        var batch3 = fixture.CreateRandomEntity<Core.Models.Batch>();
        batch3.Name = "Charlie";
        dbContext.Batches.AddRange(batch1, batch2, batch3);
        await dbContext.SaveChangesAsync();

        // Act - Real HTTP GET request with sorting query parameters
        var response = await fixture.Client.GetAsync("/api/v1/batches?sortBy=name&sortOrder=desc");
        var responseBody = await response.Content.ReadFromJsonAsync<GetBatchesListEndpoint.GetBatchesListResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Batches);
        var batchesList = responseBody.Batches.ToList();
        Assert.Equal(3, batchesList.Count);
        Assert.Equal("Charlie", batchesList[0].Name);
        Assert.Equal("Bravo", batchesList[1].Name);
        Assert.Equal("Alpha", batchesList[2].Name);
    }

    [Fact]
    public async Task GET_Batches_ShouldReturnBatchesSortedByNameAscending_WhenSortByNameAndSortOrderAsc()
    {
        // Arrange - Add batches with specific names
        var batch1 = fixture.CreateRandomEntity<Core.Models.Batch>();
        batch1.Name = "Charlie";
        var batch2 = fixture.CreateRandomEntity<Core.Models.Batch>();
        batch2.Name = "Alpha";
        var batch3 = fixture.CreateRandomEntity<Core.Models.Batch>();
        batch3.Name = "Bravo";
        dbContext.Batches.AddRange(batch1, batch2, batch3);
        await dbContext.SaveChangesAsync();

        // Act - Real HTTP GET request with sorting query parameters
        var response = await fixture.Client.GetAsync("/api/v1/batches?sortBy=name&sortOrder=asc");
        var responseBody = await response.Content.ReadFromJsonAsync<GetBatchesListEndpoint.GetBatchesListResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Batches);
        var batchesList = responseBody.Batches.ToList();
        Assert.Equal(3, batchesList.Count);
        Assert.Equal("Alpha", batchesList[0].Name);
        Assert.Equal("Bravo", batchesList[1].Name);
        Assert.Equal("Charlie", batchesList[2].Name);
    }

    [Fact]
    public async Task GET_Batches_ShouldReturnBatchesSortedByStatusDescending_WhenSortByStatusAndSortOrderDesc()
    {
        // Arrange - Add batches with different statuses
        var batch1 = fixture.CreateRandomEntity<Core.Models.Batch>();
        batch1.Status = BatchStatus.Active;
        var batch2 = fixture.CreateRandomEntity<Core.Models.Batch>();
        batch2.Status = BatchStatus.Processed;
        var batch3 = fixture.CreateRandomEntity<Core.Models.Batch>();
        batch3.Status = BatchStatus.Canceled;
        dbContext.Batches.AddRange(batch1, batch2, batch3);
        await dbContext.SaveChangesAsync();

        // Act - Real HTTP GET request with sorting query parameters
        var response = await fixture.Client.GetAsync("/api/v1/batches?sortBy=status&sortOrder=desc");
        var responseBody = await response.Content.ReadFromJsonAsync<GetBatchesListEndpoint.GetBatchesListResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Batches);
        var batchesList = responseBody.Batches.ToList();
        Assert.Equal(3, batchesList.Count);
        Assert.Equal(BatchStatus.Canceled.ToString(), batchesList[0].Status); // 4
        Assert.Equal(BatchStatus.Processed.ToString(), batchesList[1].Status); // 1
        Assert.Equal(BatchStatus.Active.ToString(), batchesList[2].Status); // 0
    }

    [Fact]
    public async Task GET_Batches_ShouldReturnBatchesInDefaultOrder_WhenNoSortingParametersProvided()
    {
        // Arrange - Add batches with different start dates
        var batch1 = fixture.CreateRandomEntity<Core.Models.Batch>();
        batch1.StartDate = System.DateTime.UtcNow.AddDays(-5);
        var batch2 = fixture.CreateRandomEntity<Core.Models.Batch>();
        batch2.StartDate = System.DateTime.UtcNow.AddDays(-10);
        var batch3 = fixture.CreateRandomEntity<Core.Models.Batch>();
        batch3.StartDate = System.DateTime.UtcNow.AddDays(-1);
        dbContext.Batches.AddRange(batch1, batch2, batch3);
        await dbContext.SaveChangesAsync();

        // Act - Real HTTP GET request without sorting parameters
        var response = await fixture.Client.GetAsync("/api/v1/batches");
        var responseBody = await response.Content.ReadFromJsonAsync<GetBatchesListEndpoint.GetBatchesListResponseBody>();

        // Assert - Should be sorted by StartDate descending (default)
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Batches);
        var batchesList = responseBody.Batches.ToList();
        Assert.Equal(3, batchesList.Count);
        Assert.Equal(batch3.Id, batchesList[0].Id); // Most recent
        Assert.Equal(batch1.Id, batchesList[1].Id);
        Assert.Equal(batch2.Id, batchesList[2].Id); // Oldest
    }

}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.Batches;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.Core.Models.BatchActivities;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetBatchesListQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetBatchesListQuery.Args, GetBatchesListQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetBatchesListQuery.Args, GetBatchesListQuery.Result>>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    // Add tests for GetBatchesListQuery here
    [Fact]
    public async Task GetBatchesListQuery_ShouldReturnEmptyList_WhenNoBatchesExist()
    {
        var request = new AppRequest<GetBatchesListQuery.Args>(new());
        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Batches);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GetBatchesListQuery_ShouldReturnBatches_WhenBatchesExist(int batchCount)
    {
        var batches = new List<Batch>();
        // Arrange - Add batches to the database
        for (int i = 0; i < batchCount; i++)
        {
            var batch = fixture.CreateRandomEntity<Batch>();
            batches.Add(batch);
            dbContext.Batches.Add(batch);
        }
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetBatchesListQuery.Args>(new());
        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(batchCount, result.Value.Batches.Count());
    }

    [Fact]
    public async Task GetBatchesListQuery_ShouldReturnBatchesWithNullFirstStatusChangeDate_WhenNoStatusSwitchActivitiesExist()
    {
        // Arrange - Add batches without status switch activities
        var batch1 = fixture.CreateRandomEntity<Batch>();
        var batch2 = fixture.CreateRandomEntity<Batch>();
        dbContext.Batches.Add(batch1);
        dbContext.Batches.Add(batch2);
        await dbContext.SaveChangesAsync();

        // Act
        var request = new AppRequest<GetBatchesListQuery.Args>(new());
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Batches.Count());
        Assert.All(result.Value.Batches, b => Assert.Null(b.FirstStatusChangeDate));
    }

    [Fact]
    public async Task GetBatchesListQuery_ShouldReturnBatchesWithFirstStatusChangeDate_WhenStatusSwitchActivitiesExist()
    {
        // Arrange - Add batches with status switch activities
        var batch1 = fixture.CreateRandomEntity<Batch>();
        var batch2 = fixture.CreateRandomEntity<Batch>();
        dbContext.Batches.Add(batch1);
        dbContext.Batches.Add(batch2);
        await dbContext.SaveChangesAsync();

        // Add status switch activities for batch1 (multiple switches)
        var statusSwitch1_1 = new StatusSwitchBatchActivity
        {
            BatchId = batch1.Id,
            Date = System.DateTime.UtcNow.AddDays(-10),
            NewStatus = BatchStatus.Processed,
            Type = BatchActivityType.StatusSwitch
        };
        var statusSwitch1_2 = new StatusSwitchBatchActivity
        {
            BatchId = batch1.Id,
            Date = System.DateTime.UtcNow.AddDays(-5),
            NewStatus = BatchStatus.ForSale,
            Type = BatchActivityType.StatusSwitch
        };
        dbContext.StatusSwitchActivities.Add(statusSwitch1_1);
        dbContext.StatusSwitchActivities.Add(statusSwitch1_2);

        // Add status switch activity for batch2 (single switch)
        var statusSwitch2 = new StatusSwitchBatchActivity
        {
            BatchId = batch2.Id,
            Date = System.DateTime.UtcNow.AddDays(-7),
            NewStatus = BatchStatus.Canceled,
            Type = BatchActivityType.StatusSwitch
        };
        dbContext.StatusSwitchActivities.Add(statusSwitch2);
        await dbContext.SaveChangesAsync();

        // Act
        var request = new AppRequest<GetBatchesListQuery.Args>(new());
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Batches.Count());

        var batch1Dto = result.Value.Batches.First(b => b.Id == batch1.Id);
        var batch2Dto = result.Value.Batches.First(b => b.Id == batch2.Id);

        // Batch1 should have the earliest status switch date (from statusSwitch1_1)
        Assert.NotNull(batch1Dto.FirstStatusChangeDate);
        Assert.Equal(statusSwitch1_1.Date.ToString(Application.Constants.DateTimeFormat), batch1Dto.FirstStatusChangeDate);

        // Batch2 should have its only status switch date
        Assert.NotNull(batch2Dto.FirstStatusChangeDate);
        Assert.Equal(statusSwitch2.Date.ToString(Application.Constants.DateTimeFormat), batch2Dto.FirstStatusChangeDate);
    }

    [Fact]
    public async Task GetBatchesListQuery_ShouldReturnMixedBatches_WithAndWithoutStatusSwitchActivities()
    {
        // Arrange - Add batches with and without status switch activities
        var batchWithActivity = fixture.CreateRandomEntity<Batch>();
        var batchWithoutActivity = fixture.CreateRandomEntity<Batch>();
        dbContext.Batches.Add(batchWithActivity);
        dbContext.Batches.Add(batchWithoutActivity);
        await dbContext.SaveChangesAsync();

        // Add status switch activity for one batch only
        var statusSwitch = new StatusSwitchBatchActivity
        {
            BatchId = batchWithActivity.Id,
            Date = System.DateTime.UtcNow.AddDays(-3),
            NewStatus = BatchStatus.Processed,
            Type = BatchActivityType.StatusSwitch
        };
        dbContext.StatusSwitchActivities.Add(statusSwitch);
        await dbContext.SaveChangesAsync();

        // Act
        var request = new AppRequest<GetBatchesListQuery.Args>(new());
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Batches.Count());

        var batchWithActivityDto = result.Value.Batches.First(b => b.Id == batchWithActivity.Id);
        var batchWithoutActivityDto = result.Value.Batches.First(b => b.Id == batchWithoutActivity.Id);

        // Batch with activity should have FirstStatusChangeDate
        Assert.NotNull(batchWithActivityDto.FirstStatusChangeDate);
        Assert.Equal(statusSwitch.Date.ToString(Application.Constants.DateTimeFormat), batchWithActivityDto.FirstStatusChangeDate);

        // Batch without activity should have null FirstStatusChangeDate
        Assert.Null(batchWithoutActivityDto.FirstStatusChangeDate);
    }
}

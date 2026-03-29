using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.Batches;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class UpdateBatchDailyFeedingTimesCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<UpdateBatchDailyFeedingTimesCommand.Args, UpdateBatchDailyFeedingTimesCommand.Result> handler =
        fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<UpdateBatchDailyFeedingTimesCommand.Args, UpdateBatchDailyFeedingTimesCommand.Result>>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task UpdateBatchDailyFeedingTimesCommand_ShouldSetDailyFeedingTimes()
    {
        // Arrange
        var batch = fixture.CreateRandomEntity<Batch>();
        batch.DailyFeedingTimes = null;
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<UpdateBatchDailyFeedingTimesCommand.Args>(new(batch.Id, 3));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.UpdatedBatch.DailyFeedingTimes);
        await dbContext.Entry(batch).ReloadAsync();
        Assert.Equal(3, batch.DailyFeedingTimes);
    }

    [Fact]
    public async Task UpdateBatchDailyFeedingTimesCommand_ShouldClearDailyFeedingTimes_WhenValueIsNull()
    {
        // Arrange
        var batch = fixture.CreateRandomEntity<Batch>();
        batch.DailyFeedingTimes = 2;
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<UpdateBatchDailyFeedingTimesCommand.Args>(new(batch.Id, null));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.UpdatedBatch.DailyFeedingTimes);
        await dbContext.Entry(batch).ReloadAsync();
        Assert.Null(batch.DailyFeedingTimes);
    }

    [Fact]
    public async Task UpdateBatchDailyFeedingTimesCommand_ShouldReturnValidationError_WhenBatchIdIsEmpty()
    {
        // Arrange
        var request = new AppRequest<UpdateBatchDailyFeedingTimesCommand.Args>(new(Guid.Empty, 2));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchId");
    }

    [Fact]
    public async Task UpdateBatchDailyFeedingTimesCommand_ShouldReturnValidationError_WhenBatchDoesNotExist()
    {
        // Arrange
        var request = new AppRequest<UpdateBatchDailyFeedingTimesCommand.Args>(new(Guid.NewGuid(), 1));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchId");
    }

    [Fact]
    public async Task UpdateBatchDailyFeedingTimesCommand_ShouldReturnValidationError_WhenValueIsLessThanOne()
    {
        // Arrange
        var batch = fixture.CreateRandomEntity<Batch>();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<UpdateBatchDailyFeedingTimesCommand.Args>(new(batch.Id, 0));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dailyFeedingTimes");
    }
}

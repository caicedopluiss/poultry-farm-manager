using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.Batches;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class AssignFeedingTableToBatchCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<AssignFeedingTableToBatchCommand.Args, AssignFeedingTableToBatchCommand.Result> handler =
        fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<AssignFeedingTableToBatchCommand.Args, AssignFeedingTableToBatchCommand.Result>>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task AssignFeedingTableToBatchCommand_ShouldAssignFeedingTable()
    {
        // Arrange
        var batch = fixture.CreateRandomEntity<Batch>();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var request = new AppRequest<AssignFeedingTableToBatchCommand.Args>(new(batch.Id, feedingTable.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await dbContext.Entry(batch).ReloadAsync();
        Assert.Equal(feedingTable.Id, batch.FeedingTableId);
    }

    [Fact]
    public async Task AssignFeedingTableToBatchCommand_ShouldUnassignFeedingTable_WhenFeedingTableIdIsNull()
    {
        // Arrange — batch already has a feeding table
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var batch = fixture.CreateRandomEntity<Batch>();
        batch.FeedingTableId = feedingTable.Id;
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<AssignFeedingTableToBatchCommand.Args>(new(batch.Id, null));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await dbContext.Entry(batch).ReloadAsync();
        Assert.Null(batch.FeedingTableId);
    }

    [Fact]
    public async Task AssignFeedingTableToBatchCommand_ShouldReturnValidationError_WhenBatchDoesNotExist()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var request = new AppRequest<AssignFeedingTableToBatchCommand.Args>(new(Guid.NewGuid(), feedingTable.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchId");
    }

    [Fact]
    public async Task AssignFeedingTableToBatchCommand_ShouldReturnValidationError_WhenFeedingTableDoesNotExist()
    {
        // Arrange
        var batch = fixture.CreateRandomEntity<Batch>();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<AssignFeedingTableToBatchCommand.Args>(new(batch.Id, Guid.NewGuid()));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "feedingTableId");
    }

    [Fact]
    public async Task AssignFeedingTableToBatchCommand_ShouldReturnValidationError_WhenBatchIdIsEmpty()
    {
        // Arrange
        var request = new AppRequest<AssignFeedingTableToBatchCommand.Args>(new(Guid.Empty, null));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchId");
    }
}

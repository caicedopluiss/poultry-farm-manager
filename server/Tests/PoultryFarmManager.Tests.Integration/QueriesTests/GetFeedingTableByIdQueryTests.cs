using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Queries.FeedingTables;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetFeedingTableByIdQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetFeedingTableByIdQuery.Args, GetFeedingTableByIdQuery.Result> handler =
        fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetFeedingTableByIdQuery.Args, GetFeedingTableByIdQuery.Result>>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GetFeedingTableByIdQuery_ShouldReturnNull_WhenFeedingTableDoesNotExist()
    {
        // Arrange
        var request = new AppRequest<GetFeedingTableByIdQuery.Args>(new(Guid.NewGuid()));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.FeedingTable);
    }

    [Fact]
    public async Task GetFeedingTableByIdQuery_ShouldReturnFeedingTable_WhenFound()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync(name: "My Table", dayCount: 3);
        var request = new AppRequest<GetFeedingTableByIdQuery.Args>(new(feedingTable.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.FeedingTable);
        Assert.Equal(feedingTable.Id, result.Value.FeedingTable.Id);
        Assert.Equal("My Table", result.Value.FeedingTable.Name);
        Assert.Equal(3, result.Value.FeedingTable.DayEntries.Count);
    }

    [Fact]
    public async Task GetFeedingTableByIdQuery_ShouldReturnDayEntriesOrderedByDayNumber()
    {
        // Arrange — entries seeded in reverse order
        var feedingTable = await dbContext.CreateFeedingTableAsync(dayCount: 5);
        var request = new AppRequest<GetFeedingTableByIdQuery.Args>(new(feedingTable.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        var entries = result.Value!.FeedingTable!.DayEntries.ToList();
        for (var i = 0; i < entries.Count - 1; i++)
            Assert.True(entries[i].DayNumber < entries[i + 1].DayNumber);
    }

    [Fact]
    public async Task GetFeedingTableByIdQuery_ShouldReturnValidationError_WhenIdIsEmpty()
    {
        // Arrange
        var request = new AppRequest<GetFeedingTableByIdQuery.Args>(new(Guid.Empty));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "feedingTableId");
    }

    [Fact]
    public async Task GetFeedingTableByIdQuery_ShouldReturnCorrectAmountPerBirdAndExpectedBirdWeight()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync(name: "Weight Table", dayCount: 1);
        var entry = feedingTable.DayEntries.First();
        entry.ExpectedBirdWeight = 1.25m;
        entry.ExpectedBirdWeightUnitOfMeasure = PoultryFarmManager.Core.Enums.UnitOfMeasure.Kilogram;
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetFeedingTableByIdQuery.Args>(new(feedingTable.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var resultEntry = Assert.Single(result.Value!.FeedingTable!.DayEntries);
        Assert.Equal(entry.AmountPerBird, resultEntry.AmountPerBird);
        Assert.Equal(1.25m, resultEntry.ExpectedBirdWeight);
        Assert.Equal("Kilogram", resultEntry.ExpectedBirdWeightUnitOfMeasure);
    }
}

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Queries.FeedingTables;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetFeedingTablesListQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetFeedingTablesListQuery.Args, GetFeedingTablesListQuery.Result> handler =
        fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetFeedingTablesListQuery.Args, GetFeedingTablesListQuery.Result>>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GetFeedingTablesListQuery_ShouldReturnEmptyList_WhenNoFeedingTablesExist()
    {
        // Arrange
        var request = new AppRequest<GetFeedingTablesListQuery.Args>(new());

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.FeedingTables);
    }

    [Fact]
    public async Task GetFeedingTablesListQuery_ShouldReturnAllFeedingTables()
    {
        // Arrange
        await dbContext.CreateFeedingTableAsync(name: "Table A", dayCount: 2);
        await dbContext.CreateFeedingTableAsync(name: "Table B", dayCount: 3);
        var request = new AppRequest<GetFeedingTablesListQuery.Args>(new());

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.FeedingTables.Count());
    }

    [Fact]
    public async Task GetFeedingTablesListQuery_ShouldReturnFeedingTablesWithDayEntries()
    {
        // Arrange
        await dbContext.CreateFeedingTableAsync(name: "Table With Entries", dayCount: 4);
        var request = new AppRequest<GetFeedingTablesListQuery.Args>(new());

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var table = result.Value!.FeedingTables.Single();
        Assert.Equal(4, table.DayEntries.Count);
    }
}

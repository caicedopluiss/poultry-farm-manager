using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.Batches;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models;

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
}

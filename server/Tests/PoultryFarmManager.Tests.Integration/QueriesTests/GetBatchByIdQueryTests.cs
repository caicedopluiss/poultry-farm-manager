using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.Batches;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetBatchByIdQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetBatchByIdQuery.Args, GetBatchByIdQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetBatchByIdQuery.Args, GetBatchByIdQuery.Result>>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GetBatchByIdQuery_ShouldReturnNull_WhenBatchDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new AppRequest<GetBatchByIdQuery.Args>(new(nonExistentId));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value.Batch);
    }

    [Fact]
    public async Task GetBatchByIdQuery_ShouldReturnBatch_WhenBatchExists()
    {
        // Arrange - Add a batch to the database
        var batch = fixture.CreateRandomEntity<Batch>();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetBatchByIdQuery.Args>(new(batch.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value.Batch);
        Assert.Equal(batch.Id, result.Value.Batch.Id);
        Assert.Equal(batch.Name, result.Value.Batch.Name);
        Assert.Equal(batch.Breed, result.Value.Batch.Breed);
        Assert.Equal(batch.Status.ToString(), result.Value.Batch.Status);
        Assert.Equal(batch.InitialPopulation, result.Value.Batch.InitialPopulation);
        Assert.Equal(batch.MaleCount, result.Value.Batch.MaleCount);
        Assert.Equal(batch.FemaleCount, result.Value.Batch.FemaleCount);
        Assert.Equal(batch.UnsexedCount, result.Value.Batch.UnsexedCount);
        Assert.Equal(batch.Shed, result.Value.Batch.Shed);
    }

    [Fact]
    public async Task GetBatchByIdQuery_ShouldReturnValidationError_WhenIdIsEmpty()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var request = new AppRequest<GetBatchByIdQuery.Args>(new(emptyId));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "Id" && e.error == "Id cannot be empty");
    }
}

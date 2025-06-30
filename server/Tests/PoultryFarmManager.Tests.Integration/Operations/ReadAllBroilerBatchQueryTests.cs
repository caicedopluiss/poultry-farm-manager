using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Operations.Queries;
using PoultryFarmManager.Core.Operations.Models;
using PoultryFarmManager.Infrastructure;
using SharedLib.CQRS;

namespace PoultryFarmManager.Tests.Integration.Operations;

[Collection(InfrastructureContextCollection.Name)]
public class ReadAllBroilerBatchQueryTests(InfrastructureContextFixture fixture) : IAsyncLifetime
{
    private readonly ApplicationDbContext dbContext = fixture.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    public async Task DisposeAsync()
    {
        dbContext.BroilerBatches.RemoveRange(dbContext.BroilerBatches);
        await dbContext.SaveChangesAsync();
    }

    public Task InitializeAsync()
    {
        dbContext.BroilerBatches.RemoveRange(dbContext.BroilerBatches);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ReadAllBroilerBatchQuery_ShouldReturnAllBatches()
    {
        // Arrange
        var existingBatches = new[]
        {
            new BroilerBatch
            {
                BatchName = "Batch 1",
                InitialPopulation = 1000,
                CurrentPopulation = 1000,
                StartDate = DateTime.UtcNow,
                Status = BroilerBatchStatus.Active,
                CreatedAt = DateTime.UtcNow,
                Notes = "First batch for testing"
            },
            new BroilerBatch
            {
                BatchName = "Batch 2",
                InitialPopulation = 2000,
                CurrentPopulation = 2000,
                StartDate = DateTime.UtcNow.AddDays(-10),
                Status = BroilerBatchStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                Notes = "Second batch for testing"
            }
        };

        await dbContext.BroilerBatches.AddRangeAsync(existingBatches);
        await dbContext.SaveChangesAsync();

        // Arrange
        dbContext.ChangeTracker.Clear();

        var serviceProvider = fixture.ServiceProvider;
        var queryHandler = serviceProvider.GetRequiredService<IAppRequestHandler<ReadAllBroilerBatchQuery.Args, ReadAllBroilerBatchQuery.Result>>();

        // Act
        var result = await queryHandler.HandleAsync(new AppRequest<ReadAllBroilerBatchQuery.Args>(new()), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value.Batches);
        Assert.Equal(existingBatches.Length, result.Value.Batches.Count());
        Assert.Contains(result.Value.Batches, b => b.BatchName == "Batch 1");
        Assert.Contains(result.Value.Batches, b => b.BatchName == "Batch 2");
        Assert.All(result.Value.Batches, b =>
        {
            Assert.NotEqual(Guid.Empty, b.Id);
            Assert.NotNull(b.BatchName);
            Assert.NotNull(b.Status);
        });
    }
}
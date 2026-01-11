using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.Batches;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class UpdateBatchNameCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<UpdateBatchNameCommand.Args, UpdateBatchNameCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<UpdateBatchNameCommand.Args, UpdateBatchNameCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UpdateBatchNameCommand_ShouldUpdateName_WithValidData()
    {
        // Arrange - Create a batch first
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch.Name = "Original Name";
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<UpdateBatchNameCommand.Args>(new(batch.Id, "Updated Name"));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(batch.Id, result.Value!.UpdatedBatch.Id);
        Assert.Equal("Updated Name", result.Value!.UpdatedBatch.Name);

        // Verify in database - clear change tracker to get fresh data
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.Equal("Updated Name", updatedBatch.Name);
    }

    [Fact]
    public async Task UpdateBatchNameCommand_ShouldFail_WithNonExistentId()
    {
        // Arrange
        var request = new AppRequest<UpdateBatchNameCommand.Args>(new(Guid.NewGuid(), "New Name"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(request, CancellationToken.None));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task UpdateBatchNameCommand_ShouldFail_WithEmptyBatchId()
    {
        // Arrange
        var request = new AppRequest<UpdateBatchNameCommand.Args>(new(Guid.Empty, "New Name"));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchId");
    }

    [Fact]
    public async Task UpdateBatchNameCommand_ShouldFail_WithEmptyName()
    {
        // Arrange - Create a batch first
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<UpdateBatchNameCommand.Args>(new(batch.Id, ""));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
    }

    [Fact]
    public async Task UpdateBatchNameCommand_ShouldFail_WithNameExceedingMaxLength()
    {
        // Arrange - Create a batch first
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<UpdateBatchNameCommand.Args>(new(batch.Id, new string('A', 101)));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
        Assert.Contains("cannot exceed 100 characters", result.ValidationErrors.First(e => e.field == "name").error);
    }

    [Fact]
    public async Task UpdateBatchNameCommand_ShouldFail_WithDuplicateName()
    {
        // Arrange - Create two batches
        var batch1 = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch1.Name = "Existing Batch Name";
        var batch2 = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch2.Name = "Batch to Update";
        dbContext.Batches.AddRange(batch1, batch2);
        await dbContext.SaveChangesAsync();

        // Try to update batch2 with batch1's name
        var request = new AppRequest<UpdateBatchNameCommand.Args>(new(batch2.Id, "Existing Batch Name"));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
        Assert.Contains("already exists", result.ValidationErrors.First(e => e.field == "name").error);
    }

    [Fact]
    public async Task UpdateBatchNameCommand_ShouldSucceed_WithSameName()
    {
        // Arrange - Create a batch
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch.Name = "Current Name";
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        // Update with the same name (should be allowed)
        var request = new AppRequest<UpdateBatchNameCommand.Args>(new(batch.Id, "Current Name"));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Current Name", result.Value!.UpdatedBatch.Name);
    }

    [Fact]
    public async Task UpdateBatchNameCommand_ShouldOnlyUpdateName_NotOtherProperties()
    {
        // Arrange - Create a batch with specific properties
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        // Save original values after persisting to database
        var originalBreed = batch.Breed;
        var originalShed = batch.Shed;
        var originalPopulation = batch.Population;

        var request = new AppRequest<UpdateBatchNameCommand.Args>(new(batch.Id, "New Name Only"));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        // Verify only name changed - clear change tracker to get fresh data
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.Equal("New Name Only", updatedBatch.Name);
        Assert.Equal(originalBreed, updatedBatch.Breed);
        Assert.Equal(originalShed, updatedBatch.Shed);
        Assert.Equal(originalPopulation, updatedBatch.Population);
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.Batches;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class UpdateBatchNotesCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<UpdateBatchNotesCommand.Args, UpdateBatchNotesCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<UpdateBatchNotesCommand.Args, UpdateBatchNotesCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UpdateBatchNotesCommand_ShouldUpdateNotes_WithValidData()
    {
        // Arrange - Create a batch first
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch.Notes = null;
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<UpdateBatchNotesCommand.Args>(new(batch.Id, "These are the batch notes"));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value!.Success);

        // Verify in database - clear change tracker to get fresh data
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.Equal("These are the batch notes", updatedBatch.Notes);
    }

    [Fact]
    public async Task UpdateBatchNotesCommand_ShouldUpdateNotes_FromExistingToNew()
    {
        // Arrange - Create a batch with existing notes
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch.Notes = "Original notes";
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<UpdateBatchNotesCommand.Args>(new(batch.Id, "Updated notes"));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        // Verify in database
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.Equal("Updated notes", updatedBatch.Notes);
    }

    [Fact]
    public async Task UpdateBatchNotesCommand_ShouldClearNotes_WithNullValue()
    {
        // Arrange - Create a batch with existing notes
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch.Notes = "Some existing notes";
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<UpdateBatchNotesCommand.Args>(new(batch.Id, null));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        // Verify in database
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.Null(updatedBatch.Notes);
    }

    [Fact]
    public async Task UpdateBatchNotesCommand_ShouldClearNotes_WhenEmptyStringProvided()
    {
        // Arrange - Create a batch with existing notes
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        batch.Notes = "Some existing notes";
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<UpdateBatchNotesCommand.Args>(new(batch.Id, ""));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        // Verify in database - empty string should be converted to null
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.Null(updatedBatch.Notes);
    }

    [Fact]
    public async Task UpdateBatchNotesCommand_ShouldFail_WithNonExistentId()
    {
        // Arrange
        var request = new AppRequest<UpdateBatchNotesCommand.Args>(new(Guid.NewGuid(), "Some notes"));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchId" && e.error.Contains("not found"));
    }

    [Fact]
    public async Task UpdateBatchNotesCommand_ShouldFail_WithEmptyBatchId()
    {
        // Arrange
        var request = new AppRequest<UpdateBatchNotesCommand.Args>(new(Guid.Empty, "Some notes"));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchId");
    }

    [Fact]
    public async Task UpdateBatchNotesCommand_ShouldFail_WithNotesExceedingMaxLength()
    {
        // Arrange - Create a batch first
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<UpdateBatchNotesCommand.Args>(new(batch.Id, new string('A', 501)));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "notes");
        Assert.Contains("cannot exceed 500 characters", result.ValidationErrors.First(e => e.field == "notes").error);
    }

    [Fact]
    public async Task UpdateBatchNotesCommand_ShouldSucceed_WithMaxLengthNotes()
    {
        // Arrange - Create a batch first
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var maxLengthNotes = new string('A', 500);
        var request = new AppRequest<UpdateBatchNotesCommand.Args>(new(batch.Id, maxLengthNotes));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        // Verify in database
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.NotNull(updatedBatch.Notes);
        Assert.Equal(maxLengthNotes, updatedBatch.Notes);
        Assert.Equal(500, updatedBatch.Notes.Length);
    }

    [Fact]
    public async Task UpdateBatchNotesCommand_ShouldOnlyUpdateNotes_NotOtherProperties()
    {
        // Arrange - Create a batch with specific properties
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        // Save original values after persisting to database
        var originalName = batch.Name;
        var originalBreed = batch.Breed;
        var originalShed = batch.Shed;
        var originalPopulation = batch.Population;
        var originalStatus = batch.Status;

        var request = new AppRequest<UpdateBatchNotesCommand.Args>(new(batch.Id, "New notes only"));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        // Verify only notes changed - clear change tracker to get fresh data
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.Equal("New notes only", updatedBatch.Notes);
        Assert.Equal(originalName, updatedBatch.Name);
        Assert.Equal(originalBreed, updatedBatch.Breed);
        Assert.Equal(originalShed, updatedBatch.Shed);
        Assert.Equal(originalPopulation, updatedBatch.Population);
        Assert.Equal(originalStatus, updatedBatch.Status);
    }

    [Fact]
    public async Task UpdateBatchNotesCommand_ShouldHandleMultilineNotes()
    {
        // Arrange - Create a batch first
        var batch = TestEntityFactory.GetFactory<Batch>().CreateRandom();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var multilineNotes = "Line 1\nLine 2\nLine 3\nLine 4";
        var request = new AppRequest<UpdateBatchNotesCommand.Args>(new(batch.Id, multilineNotes));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        // Verify in database
        dbContext.ChangeTracker.Clear();
        var updatedBatch = await dbContext.Batches.FindAsync(batch.Id);
        Assert.NotNull(updatedBatch);
        Assert.Equal(multilineNotes, updatedBatch.Notes);
    }
}

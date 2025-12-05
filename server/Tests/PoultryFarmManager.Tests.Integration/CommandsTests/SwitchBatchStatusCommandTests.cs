using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Commands.Batches;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class SwitchBatchStatusCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<SwitchBatchStatusCommand.Args, SwitchBatchStatusCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<SwitchBatchStatusCommand.Args, SwitchBatchStatusCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SwitchBatchStatusCommand_ShouldSwitchFromActiveToProcessed()
    {
        // Arrange - Create an active batch
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Status Switch",
            Breed = "Layer",
            StartDate = DateTime.UtcNow.AddDays(-30),
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = BatchStatus.Active,
            Shed = "Shed B-2"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        var statusSwitch = new NewStatusSwitchDto
        (
            NewStatus: "Processed",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Processing completed"
        );
        var request = new AppRequest<SwitchBatchStatusCommand.Args>(new(batchId, statusSwitch));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Detach and reload
        dbContext.Entry(batch).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        var updatedBatch = await dbContext.Batches.FindAsync(batchId);
        var statusSwitchRecord = await dbContext.StatusSwitchActivities.FindAsync(result.Value!.StatusSwitch.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.StatusSwitch.Id);
        Assert.Equal(batchId, result.Value!.StatusSwitch.BatchId);
        Assert.Equal("Processed", result.Value!.StatusSwitch.NewStatus);
        Assert.Equal("Processing completed", result.Value!.StatusSwitch.Notes);

        // Verify batch status was updated
        Assert.NotNull(updatedBatch);
        Assert.Equal(BatchStatus.Processed, updatedBatch!.Status);

        // Verify activity record was created
        Assert.NotNull(statusSwitchRecord);
        Assert.Equal(BatchStatus.Processed, statusSwitchRecord!.NewStatus);
    }

    [Fact]
    public async Task SwitchBatchStatusCommand_ShouldSwitchFromActiveToForSale()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Direct to ForSale",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow.AddDays(-45),
            MaleCount = 80,
            FemaleCount = 85,
            UnsexedCount = 5,
            InitialPopulation = 170,
            Status = BatchStatus.Active,
            Shed = "Shed C-3"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        var statusSwitch = new NewStatusSwitchDto
        (
            NewStatus: "ForSale",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Ready for market"
        );
        var request = new AppRequest<SwitchBatchStatusCommand.Args>(new(batchId, statusSwitch));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Detach and reload
        dbContext.Entry(batch).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        var updatedBatch = await dbContext.Batches.FindAsync(batchId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("ForSale", result.Value!.StatusSwitch.NewStatus);
        Assert.Equal(BatchStatus.ForSale, updatedBatch!.Status);
    }

    [Fact]
    public async Task SwitchBatchStatusCommand_ShouldSwitchFromProcessedToForSale()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Processed to ForSale",
            Breed = "Mixed",
            StartDate = DateTime.UtcNow.AddDays(-60),
            MaleCount = 70,
            FemaleCount = 70,
            UnsexedCount = 0,
            InitialPopulation = 140,
            Status = BatchStatus.Processed,
            Shed = "Shed D-4"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        var statusSwitch = new NewStatusSwitchDto
        (
            NewStatus: "ForSale",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<SwitchBatchStatusCommand.Args>(new(batchId, statusSwitch));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Detach and reload
        dbContext.Entry(batch).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        var updatedBatch = await dbContext.Batches.FindAsync(batchId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("ForSale", result.Value!.StatusSwitch.NewStatus);
        Assert.Equal(BatchStatus.ForSale, updatedBatch!.Status);
    }

    [Fact]
    public async Task SwitchBatchStatusCommand_ShouldSwitchFromForSaleToSold()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch ForSale to Sold",
            Breed = "Layer",
            StartDate = DateTime.UtcNow.AddDays(-75),
            MaleCount = 60,
            FemaleCount = 60,
            UnsexedCount = 0,
            InitialPopulation = 120,
            Status = BatchStatus.ForSale,
            Shed = "Shed E-5"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        var statusSwitch = new NewStatusSwitchDto
        (
            NewStatus: "Sold",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Sold to buyer"
        );
        var request = new AppRequest<SwitchBatchStatusCommand.Args>(new(batchId, statusSwitch));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Detach and reload
        dbContext.Entry(batch).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        var updatedBatch = await dbContext.Batches.FindAsync(batchId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Sold", result.Value!.StatusSwitch.NewStatus);
        Assert.Equal(BatchStatus.Sold, updatedBatch!.Status);
    }

    [Fact]
    public async Task SwitchBatchStatusCommand_ShouldFailForInvalidTransition()
    {
        // Arrange - Try to go from Processed to Active (invalid)
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Invalid Transition",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow.AddDays(-30),
            MaleCount = 50,
            FemaleCount = 50,
            UnsexedCount = 0,
            InitialPopulation = 100,
            Status = BatchStatus.Processed,
            Shed = "Shed F-6"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        var statusSwitch = new NewStatusSwitchDto
        (
            NewStatus: "Active",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Trying to go back"
        );
        var request = new AppRequest<SwitchBatchStatusCommand.Args>(new(batchId, statusSwitch));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "newStatus");
    }

    [Fact]
    public async Task SwitchBatchStatusCommand_ShouldFailForNonExistentBatch()
    {
        // Arrange
        var nonExistentBatchId = Guid.NewGuid();
        var statusSwitch = new NewStatusSwitchDto
        (
            NewStatus: "Processed",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<SwitchBatchStatusCommand.Args>(new(nonExistentBatchId, statusSwitch));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task SwitchBatchStatusCommand_ShouldCreateMultipleStatusSwitches()
    {
        // Arrange - Create batch and switch multiple times
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Multiple Switches",
            Breed = "Mixed",
            StartDate = DateTime.UtcNow.AddDays(-90),
            MaleCount = 40,
            FemaleCount = 40,
            UnsexedCount = 0,
            InitialPopulation = 80,
            Status = BatchStatus.Active,
            Shed = "Shed G-7"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        // Act - Switch Active -> Processed
        var firstSwitch = new NewStatusSwitchDto("Processed", DateTime.UtcNow.ToString(Constants.DateTimeFormat), "First switch");
        var firstRequest = new AppRequest<SwitchBatchStatusCommand.Args>(new(batchId, firstSwitch));
        var firstResult = await handler.HandleAsync(firstRequest, CancellationToken.None);

        dbContext.Entry(batch).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

        // Act - Switch Processed -> ForSale
        var secondSwitch = new NewStatusSwitchDto("ForSale", DateTime.UtcNow.ToString(Constants.DateTimeFormat), "Second switch");
        var secondRequest = new AppRequest<SwitchBatchStatusCommand.Args>(new(batchId, secondSwitch));
        var secondResult = await handler.HandleAsync(secondRequest, CancellationToken.None);

        // Get all status switches
        var allSwitches = dbContext.StatusSwitchActivities
            .Where(s => s.BatchId == batchId)
            .OrderBy(s => s.Date)
            .ToList();

        // Assert
        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.Equal(2, allSwitches.Count);

        Assert.Equal(BatchStatus.Processed, allSwitches[0].NewStatus);
        Assert.Equal(BatchStatus.ForSale, allSwitches[1].NewStatus);
    }
}

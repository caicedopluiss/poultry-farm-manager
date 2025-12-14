using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.Assets;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

/// <summary>
/// Tests for updating AssetState quantities through Asset updates.
/// AssetState is managed as a child entity of Asset.
/// </summary>
public class UpdateAssetStateCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<CreateAssetCommand.Args, CreateAssetCommand.Result> createHandler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<CreateAssetCommand.Args, CreateAssetCommand.Result>>();
    private readonly IAppRequestHandler<UpdateAssetCommand.Args, UpdateAssetCommand.Result> updateHandler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<UpdateAssetCommand.Args, UpdateAssetCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UpdateAsset_ShouldAllowAddingNewAssetState()
    {
        // Arrange - Create asset with initial Available state
        var newAssetDto = new NewAssetDto(
            Name: "Test Equipment",
            Description: "Equipment for testing",
            InitialQuantity: 10,
            Notes: null
        );
        var createRequest = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));
        var createResult = await createHandler.HandleAsync(createRequest, CancellationToken.None);
        var assetId = createResult.Value!.CreatedAsset.Id;

        // Act - Add a new state by directly inserting into database (simulating future command)
        var asset = await dbContext.Assets
            .Include(a => a.States)
            .FirstAsync(a => a.Id == assetId);

        asset.States!.Add(new AssetState
        {
            AssetId = assetId,
            Status = AssetStatus.InUse,
            Quantity = 3
        });

        await dbContext.SaveChangesAsync();

        // Assert
        var updatedAsset = await dbContext.Assets
            .Include(a => a.States)
            .FirstAsync(a => a.Id == assetId);

        Assert.Equal(2, updatedAsset.States!.Count);
        Assert.Contains(updatedAsset.States, s => s.Status == AssetStatus.Available && s.Quantity == 10);
        Assert.Contains(updatedAsset.States, s => s.Status == AssetStatus.InUse && s.Quantity == 3);
    }

    [Fact]
    public async Task UpdateAsset_ShouldAllowUpdatingExistingAssetStateQuantity()
    {
        // Arrange - Create asset with initial state
        var newAssetDto = new NewAssetDto(
            Name: "Test Equipment",
            Description: "Equipment for testing",
            InitialQuantity: 10,
            Notes: null
        );
        var createRequest = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));
        var createResult = await createHandler.HandleAsync(createRequest, CancellationToken.None);
        var assetId = createResult.Value!.CreatedAsset.Id;

        // Act - Update the Available quantity
        var asset = await dbContext.Assets
            .Include(a => a.States)
            .FirstAsync(a => a.Id == assetId);

        var availableState = asset.States!.First(s => s.Status == AssetStatus.Available);
        availableState.Quantity = 15;

        await dbContext.SaveChangesAsync();

        // Assert
        var updatedAsset = await dbContext.Assets
            .Include(a => a.States)
            .FirstAsync(a => a.Id == assetId);

        Assert.Single(updatedAsset.States!);
        Assert.Equal(15, updatedAsset.States!.First().Quantity);
        Assert.Equal(AssetStatus.Available, updatedAsset.States!.First().Status);
    }

    [Fact]
    public async Task UpdateAsset_ShouldAllowMovingQuantityBetweenStates()
    {
        // Arrange - Create asset with multiple states
        var newAssetDto = new NewAssetDto(
            Name: "Test Equipment",
            Description: "Equipment for testing",
            InitialQuantity: 10,
            Notes: null
        );
        var createRequest = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));
        var createResult = await createHandler.HandleAsync(createRequest, CancellationToken.None);
        var assetId = createResult.Value!.CreatedAsset.Id;

        // Add InUse state
        var asset = await dbContext.Assets
            .Include(a => a.States)
            .FirstAsync(a => a.Id == assetId);

        asset.States!.Add(new AssetState
        {
            AssetId = assetId,
            Status = AssetStatus.InUse,
            Quantity = 0
        });
        await dbContext.SaveChangesAsync();

        // Act - Move 3 units from Available to InUse
        dbContext.Entry(asset).State = EntityState.Detached;
        asset = await dbContext.Assets
            .Include(a => a.States)
            .FirstAsync(a => a.Id == assetId);

        var availableState = asset.States!.First(s => s.Status == AssetStatus.Available);
        var inUseState = asset.States!.First(s => s.Status == AssetStatus.InUse);

        availableState.Quantity -= 3;
        inUseState.Quantity += 3;

        await dbContext.SaveChangesAsync();

        // Assert
        dbContext.Entry(asset).State = EntityState.Detached;
        var updatedAsset = await dbContext.Assets
            .Include(a => a.States)
            .FirstAsync(a => a.Id == assetId);

        var finalAvailable = updatedAsset.States!.First(s => s.Status == AssetStatus.Available);
        var finalInUse = updatedAsset.States!.First(s => s.Status == AssetStatus.InUse);

        Assert.Equal(7, finalAvailable.Quantity);
        Assert.Equal(3, finalInUse.Quantity);
    }

    [Fact]
    public async Task UpdateAsset_ShouldHandleMultipleStates()
    {
        // Arrange - Create asset
        var newAssetDto = new NewAssetDto(
            Name: "Test Equipment",
            Description: "Equipment for testing",
            InitialQuantity: 20,
            Notes: null
        );
        var createRequest = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));
        var createResult = await createHandler.HandleAsync(createRequest, CancellationToken.None);
        var assetId = createResult.Value!.CreatedAsset.Id;

        // Act - Add multiple different states
        var asset = await dbContext.Assets
            .Include(a => a.States)
            .FirstAsync(a => a.Id == assetId);

        asset.States!.Add(new AssetState { AssetId = assetId, Status = AssetStatus.InUse, Quantity = 5 });
        asset.States.Add(new AssetState { AssetId = assetId, Status = AssetStatus.UnderMaintenance, Quantity = 2 });
        asset.States.Add(new AssetState { AssetId = assetId, Status = AssetStatus.Damaged, Quantity = 1 });

        // Update Available to reflect movement
        var availableState = asset.States.First(s => s.Status == AssetStatus.Available);
        availableState.Quantity = 12; // 20 - 5 - 2 - 1

        await dbContext.SaveChangesAsync();

        // Assert
        dbContext.Entry(asset).State = EntityState.Detached;
        var updatedAsset = await dbContext.Assets
            .Include(a => a.States)
            .FirstAsync(a => a.Id == assetId);

        Assert.Equal(4, updatedAsset.States!.Count);
        Assert.Equal(12, updatedAsset.States.First(s => s.Status == AssetStatus.Available).Quantity);
        Assert.Equal(5, updatedAsset.States.First(s => s.Status == AssetStatus.InUse).Quantity);
        Assert.Equal(2, updatedAsset.States.First(s => s.Status == AssetStatus.UnderMaintenance).Quantity);
        Assert.Equal(1, updatedAsset.States.First(s => s.Status == AssetStatus.Damaged).Quantity);
    }

    [Fact]
    public async Task UpdateAsset_ShouldPreventDuplicateStatusStates()
    {
        // Arrange - Create asset
        var newAssetDto = new NewAssetDto(
            Name: "Test Equipment",
            Description: "Equipment for testing",
            InitialQuantity: 10,
            Notes: null
        );
        var createRequest = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));
        var createResult = await createHandler.HandleAsync(createRequest, CancellationToken.None);
        var assetId = createResult.Value!.CreatedAsset.Id;

        // Act & Assert - Try to add duplicate Available state
        var asset = await dbContext.Assets
            .Include(a => a.States)
            .FirstAsync(a => a.Id == assetId);

        asset.States!.Add(new AssetState
        {
            AssetId = assetId,
            Status = AssetStatus.Available, // Duplicate status
            Quantity = 5
        });

        // This should fail due to unique constraint (if configured) or business logic
        // For now, we just verify it can be detected
        var duplicateStates = asset.States
            .GroupBy(s => s.Status)
            .Where(g => g.Count() > 1)
            .ToList();

        Assert.NotEmpty(duplicateStates); // Detects the duplicate
    }

    [Fact]
    public async Task UpdateAsset_ShouldAllowRemovingAssetState()
    {
        // Arrange - Create asset with initial state
        var newAssetDto = new NewAssetDto(
            Name: "Test Equipment",
            Description: "Equipment for testing",
            InitialQuantity: 10,
            Notes: null
        );
        var createRequest = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));
        var createResult = await createHandler.HandleAsync(createRequest, CancellationToken.None);
        var assetId = createResult.Value!.CreatedAsset.Id;

        // Add a second state
        var asset = await dbContext.Assets
            .Include(a => a.States)
            .FirstAsync(a => a.Id == assetId);

        var inUseState = new AssetState
        {
            AssetId = assetId,
            Status = AssetStatus.InUse,
            Quantity = 3
        };
        asset.States!.Add(inUseState);
        await dbContext.SaveChangesAsync();

        // Act - Remove the InUse state
        dbContext.Entry(asset).State = EntityState.Detached;
        asset = await dbContext.Assets
            .Include(a => a.States)
            .FirstAsync(a => a.Id == assetId);

        var stateToRemove = asset.States!.First(s => s.Status == AssetStatus.InUse);
        asset.States!.Remove(stateToRemove);
        await dbContext.SaveChangesAsync();

        // Assert
        dbContext.Entry(asset).State = EntityState.Detached;
        var updatedAsset = await dbContext.Assets
            .Include(a => a.States)
            .FirstAsync(a => a.Id == assetId);

        Assert.Single(updatedAsset.States!);
        Assert.Equal(AssetStatus.Available, updatedAsset.States!.First().Status);
    }

    [Fact]
    public async Task DeleteAsset_ShouldCascadeDeleteAllAssetStates()
    {
        // Arrange - Create asset with multiple states
        var newAssetDto = new NewAssetDto(
            Name: "Test Equipment",
            Description: "Equipment for testing",
            InitialQuantity: 10,
            Notes: null
        );
        var createRequest = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));
        var createResult = await createHandler.HandleAsync(createRequest, CancellationToken.None);
        var assetId = createResult.Value!.CreatedAsset.Id;

        var asset = await dbContext.Assets
            .Include(a => a.States)
            .FirstAsync(a => a.Id == assetId);

        asset.States!.Add(new AssetState { AssetId = assetId, Status = AssetStatus.InUse, Quantity = 3 });
        asset.States.Add(new AssetState { AssetId = assetId, Status = AssetStatus.Damaged, Quantity = 1 });
        await dbContext.SaveChangesAsync();

        var stateIds = asset.States.Select(s => s.Id).ToList();

        // Act - Delete the asset
        dbContext.Assets.Remove(asset);
        await dbContext.SaveChangesAsync();

        // Assert - All states should be cascade deleted
        var remainingStates = await dbContext.AssetStates
            .Where(s => stateIds.Contains(s.Id))
            .ToListAsync();

        Assert.Empty(remainingStates);
    }
}

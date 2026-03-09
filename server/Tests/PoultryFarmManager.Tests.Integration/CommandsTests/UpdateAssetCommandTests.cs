using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Commands.Assets;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class UpdateAssetCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<UpdateAssetCommand.Args, UpdateAssetCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<UpdateAssetCommand.Args, UpdateAssetCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UpdateAssetCommand_ShouldUpdateAsset_WithValidData()
    {
        // Arrange - Create an asset first
        var asset = new Asset
        {
            Name = "Old Name",
            Description = "Old description",
            Notes = "Old notes"
        };
        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateAssetDto(
            Name: "Updated Name",
            Description: "Updated description",
            Notes: "Updated notes",
            States: null
        );
        var request = new AppRequest<UpdateAssetCommand.Args>(new(asset.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(asset.Id, result.Value!.UpdatedAsset.Id);
        Assert.Equal("Updated Name", result.Value!.UpdatedAsset.Name);
        Assert.Equal("Updated description", result.Value!.UpdatedAsset.Description);
        Assert.Equal("Updated notes", result.Value!.UpdatedAsset.Notes);
    }

    [Fact]
    public async Task UpdateAssetCommand_ShouldFail_WithNonExistentId()
    {
        // Arrange
        var updateDto = new UpdateAssetDto(
            Name: "Updated Name",
            Description: null,
            Notes: null,
            States: null
        );
        var request = new AppRequest<UpdateAssetCommand.Args>(new(Guid.NewGuid(), updateDto));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task UpdateAssetCommand_ShouldFail_WithNameExceedingMaxLength()
    {
        // Arrange - Create an asset first
        var asset = new Asset
        {
            Name = "Original Name",
            Description = "Original description"
        };
        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateAssetDto(
            Name: new string('A', 101), // 101 characters - exceeds max length of 100
            Description: null,
            Notes: null,
            States: null
        );
        var request = new AppRequest<UpdateAssetCommand.Args>(new(asset.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task UpdateAssetCommand_ShouldFail_WithDescriptionExceedingMaxLength()
    {
        // Arrange - Create an asset first
        var asset = new Asset
        {
            Name = "Original Name",
            Description = "Original description"
        };
        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateAssetDto(
            Name: null,
            Description: new string('B', 501), // 501 characters - exceeds max length of 500
            Notes: null,
            States: null
        );
        var request = new AppRequest<UpdateAssetCommand.Args>(new(asset.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "description");
        Assert.Single(result.ValidationErrors);
    }



    [Fact]
    public async Task UpdateAssetCommand_ShouldFail_WithNotesExceedingMaxLength()
    {
        // Arrange - Create an asset first
        var asset = new Asset
        {
            Name = "Original Name",
            Description = "Original description"
        };
        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateAssetDto(
            Name: null,
            Description: null,
            Notes: new string('D', 501), // 501 characters - exceeds max length of 500
            States: null
        );
        var request = new AppRequest<UpdateAssetCommand.Args>(new(asset.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "notes");
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task UpdateAssetCommand_ShouldFail_WithMultipleValidationErrors()
    {
        // Arrange - Create an asset first
        var asset = new Asset
        {
            Name = "Original Name",
            Description = "Original description"
        };
        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateAssetDto(
            Name: new string('A', 101), // Name too long
            Description: new string('B', 501), // Description too long
            Notes: new string('D', 501), // Notes too long
            States: null
        );
        var request = new AppRequest<UpdateAssetCommand.Args>(new(asset.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
        Assert.Contains(result.ValidationErrors, e => e.field == "description");
        Assert.Contains(result.ValidationErrors, e => e.field == "notes");
        Assert.Equal(3, result.ValidationErrors.Count());
    }

    [Fact]
    public async Task UpdateAssetCommand_ShouldClearDescription_WhenEmptyStringProvided()
    {
        // Arrange - Create an asset with description
        var asset = new Asset
        {
            Name = "Test Asset",
            Description = "Some description",
            Notes = "Some notes"
        };
        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateAssetDto(
            Name: null,
            Description: "", // Empty string should clear description
            Notes: null,
            States: null
        );
        var request = new AppRequest<UpdateAssetCommand.Args>(new(asset.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.UpdatedAsset.Description); // Should be null
        Assert.Equal("Some notes", result.Value!.UpdatedAsset.Notes); // Should remain unchanged
    }

    [Fact]
    public async Task UpdateAssetCommand_ShouldClearNotes_WhenEmptyStringProvided()
    {
        // Arrange - Create an asset with notes
        var asset = new Asset
        {
            Name = "Test Asset",
            Description = "Some description",
            Notes = "Some notes"
        };
        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateAssetDto(
            Name: null,
            Description: null,
            Notes: "", // Empty string should clear notes
            States: null
        );
        var request = new AppRequest<UpdateAssetCommand.Args>(new(asset.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("Some description", result.Value!.UpdatedAsset.Description); // Should remain unchanged
        Assert.Null(result.Value!.UpdatedAsset.Notes); // Should be null
    }

    [Fact]
    public async Task UpdateAssetCommand_ShouldClearBothDescriptionAndNotes_WhenEmptyStringsProvided()
    {
        // Arrange - Create an asset with both fields
        var asset = new Asset
        {
            Name = "Test Asset",
            Description = "Some description",
            Notes = "Some notes"
        };
        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateAssetDto(
            Name: null,
            Description: "", // Empty string should clear description
            Notes: "   ", // Whitespace should also clear notes
            States: null
        );
        var request = new AppRequest<UpdateAssetCommand.Args>(new(asset.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.UpdatedAsset.Description); // Should be null
        Assert.Null(result.Value!.UpdatedAsset.Notes); // Should be null
    }
}


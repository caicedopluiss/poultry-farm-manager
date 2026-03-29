using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.FeedingTables;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class UpdateFeedingTableCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<UpdateFeedingTableCommand.Args, UpdateFeedingTableCommand.Result> handler =
        fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<UpdateFeedingTableCommand.Args, UpdateFeedingTableCommand.Result>>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldUpdateName()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var updates = new UpdateFeedingTableDto(Name: "Updated Name", Description: null, DayEntries: null);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(feedingTable.Id, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Updated Name", result.Value!.UpdatedFeedingTable.Name);
    }

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldReplaceDayEntries()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync(dayCount: 3);
        var newEntries = new System.Collections.Generic.List<NewFeedingTableDayEntryDto>
        {
            new(1, "Engorde", 500m, "Kilogram", null, null),
            new(2, "Engorde", 600m, "Kilogram", null, null),
        };
        var updates = new UpdateFeedingTableDto(Name: null, Description: null, DayEntries: newEntries);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(feedingTable.Id, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.UpdatedFeedingTable.DayEntries.Count);
    }

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldReturnValidationError_WhenFeedingTableDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updates = new UpdateFeedingTableDto(Name: "New Name", Description: null, DayEntries: null);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(nonExistentId, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "feedingTableId");
    }

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldReturnValidationError_WhenNameAlreadyExistsOnAnotherTable()
    {
        // Arrange
        await dbContext.CreateFeedingTableAsync(name: "Taken Name");
        var tableToUpdate = await dbContext.CreateFeedingTableAsync(name: "Original Name");
        var updates = new UpdateFeedingTableDto(Name: "Taken Name", Description: null, DayEntries: null);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(tableToUpdate.Id, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
    }

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldReturnValidationError_WhenDayEntriesListIsEmpty()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var updates = new UpdateFeedingTableDto(Name: null, Description: null, DayEntries: []);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(feedingTable.Id, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dayEntries");
    }

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldReturnValidationError_WhenFeedingTableIdIsEmpty()
    {
        // Arrange
        var updates = new UpdateFeedingTableDto(Name: "Any", Description: null, DayEntries: null);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(Guid.Empty, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "feedingTableId");
    }

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldReturnValidationError_WhenNameIsWhitespace()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var updates = new UpdateFeedingTableDto(Name: "   ", Description: null, DayEntries: null);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(feedingTable.Id, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
    }

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldReturnValidationError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var updates = new UpdateFeedingTableDto(Name: new string('A', 101), Description: null, DayEntries: null);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(feedingTable.Id, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
    }

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldReturnValidationError_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var updates = new UpdateFeedingTableDto(Name: null, Description: new string('D', 501), DayEntries: null);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(feedingTable.Id, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "description");
    }

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldAllowRenamingToOwnCurrentName()
    {
        // Arrange — renaming to itself should not trigger the uniqueness error
        var feedingTable = await dbContext.CreateFeedingTableAsync(name: "Same Name");
        var updates = new UpdateFeedingTableDto(Name: "Same Name", Description: null, DayEntries: null);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(feedingTable.Id, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldReturnValidationError_WhenReplacementDayNumberIsLessThanOne()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var updates = new UpdateFeedingTableDto(Name: null, Description: null, DayEntries:
        [
            new NewFeedingTableDayEntryDto(0, "PreInicio", 100m, "Gram", null, null),
        ]);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(feedingTable.Id, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dayEntries");
    }

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldReturnValidationError_WhenReplacementDayNumbersAreDuplicated()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var updates = new UpdateFeedingTableDto(Name: null, Description: null, DayEntries:
        [
            new NewFeedingTableDayEntryDto(1, "PreInicio", 100m, "Gram", null, null),
            new NewFeedingTableDayEntryDto(1, "Inicio", 150m, "Gram", null, null),
        ]);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(feedingTable.Id, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dayEntries");
    }

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldReturnValidationError_WhenReplacementAmountIsZero()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var updates = new UpdateFeedingTableDto(Name: null, Description: null, DayEntries:
        [
            new NewFeedingTableDayEntryDto(1, "PreInicio", 0m, "Gram", null, null),
        ]);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(feedingTable.Id, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dayEntries");
    }

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldReturnValidationError_WhenReplacementFoodTypeIsInvalid()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var updates = new UpdateFeedingTableDto(Name: null, Description: null, DayEntries:
        [
            new NewFeedingTableDayEntryDto(1, "InvalidType", 100m, "Gram", null, null),
        ]);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(feedingTable.Id, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dayEntries");
    }

    [Fact]
    public async Task UpdateFeedingTableCommand_ShouldReturnValidationError_WhenReplacementUnitOfMeasureIsNotWeight()
    {
        // Arrange
        var feedingTable = await dbContext.CreateFeedingTableAsync();
        var updates = new UpdateFeedingTableDto(Name: null, Description: null, DayEntries:
        [
            new NewFeedingTableDayEntryDto(1, "PreInicio", 100m, "Liter", null, null),
        ]);
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(feedingTable.Id, updates));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dayEntries");
    }
}

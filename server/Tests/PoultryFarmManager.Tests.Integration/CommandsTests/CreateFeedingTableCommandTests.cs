using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.FeedingTables;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class CreateFeedingTableCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<CreateFeedingTableCommand.Args, CreateFeedingTableCommand.Result> handler =
        fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<CreateFeedingTableCommand.Args, CreateFeedingTableCommand.Result>>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldCreateFeedingTable()
    {
        // Arrange
        var dto = new NewFeedingTableDto(
            Name: "Starter Table",
            Description: "A test feeding table",
            DayEntries:
            [
                new NewFeedingTableDayEntryDto(1, "PreInicio", 100m, "Gram", null, null),
                new NewFeedingTableDayEntryDto(2, "Inicio", 150m, "Gram", null, null),
                new NewFeedingTableDayEntryDto(3, "Engorde", 200m, "Kilogram", null, null),
            ]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.CreatedFeedingTable.Id);
        Assert.Equal("Starter Table", result.Value.CreatedFeedingTable.Name);
        Assert.Equal("A test feeding table", result.Value.CreatedFeedingTable.Description);
        Assert.Equal(3, result.Value.CreatedFeedingTable.DayEntries.Count);
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldReturnValidationError_WhenNameIsEmpty()
    {
        // Arrange
        var dto = new NewFeedingTableDto(
            Name: "",
            Description: null,
            DayEntries: [new NewFeedingTableDayEntryDto(1, "PreInicio", 100m, "Gram", null, null)]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldReturnValidationError_WhenNameAlreadyExists()
    {
        // Arrange
        await dbContext.CreateFeedingTableAsync(name: "Duplicate Name");
        var dto = new NewFeedingTableDto(
            Name: "Duplicate Name",
            Description: null,
            DayEntries: [new NewFeedingTableDayEntryDto(1, "PreInicio", 100m, "Gram", null, null)]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldReturnValidationError_WhenDayEntriesIsEmpty()
    {
        // Arrange
        var dto = new NewFeedingTableDto(
            Name: "Empty Entries Table",
            Description: null,
            DayEntries: []);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dayEntries");
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldReturnValidationError_WhenDayNumbersAreDuplicated()
    {
        // Arrange
        var dto = new NewFeedingTableDto(
            Name: "Dup Day Table",
            Description: null,
            DayEntries:
            [
                new NewFeedingTableDayEntryDto(1, "PreInicio", 100m, "Gram", null, null),
                new NewFeedingTableDayEntryDto(1, "Inicio", 150m, "Gram", null, null),
            ]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dayEntries");
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldReturnValidationError_WhenFoodTypeIsInvalid()
    {
        // Arrange
        var dto = new NewFeedingTableDto(
            Name: "Invalid FoodType Table",
            Description: null,
            DayEntries: [new NewFeedingTableDayEntryDto(1, "InvalidFoodType", 100m, "Gram", null, null)]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dayEntries");
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldReturnValidationError_WhenUnitOfMeasureIsNotWeight()
    {
        // Arrange — Liter is valid enum but not a weight unit
        var dto = new NewFeedingTableDto(
            Name: "Non-Weight UoM Table",
            Description: null,
            DayEntries: [new NewFeedingTableDayEntryDto(1, "PreInicio", 100m, "Liter", null, null)]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dayEntries");
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldReturnValidationError_WhenAmountIsZero()
    {
        // Arrange
        var dto = new NewFeedingTableDto(
            Name: "Zero Amount Table",
            Description: null,
            DayEntries: [new NewFeedingTableDayEntryDto(1, "PreInicio", 0m, "Gram", null, null)]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dayEntries");
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldReturnValidationError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var dto = new NewFeedingTableDto(
            Name: new string('A', 101),
            Description: null,
            DayEntries: [new NewFeedingTableDayEntryDto(1, "PreInicio", 100m, "Gram", null, null)]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldReturnValidationError_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var dto = new NewFeedingTableDto(
            Name: "Valid Name",
            Description: new string('D', 501),
            DayEntries: [new NewFeedingTableDayEntryDto(1, "PreInicio", 100m, "Gram", null, null)]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "description");
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldReturnValidationError_WhenDayNumberIsLessThanOne()
    {
        // Arrange
        var dto = new NewFeedingTableDto(
            Name: "Invalid Day Number Table",
            Description: null,
            DayEntries: [new NewFeedingTableDayEntryDto(0, "PreInicio", 100m, "Gram", null, null)]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dayEntries");
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldReturnValidationError_WhenUnitOfMeasureStringIsInvalid()
    {
        // Arrange — completely unrecognised string, not just a non-weight unit
        var dto = new NewFeedingTableDto(
            Name: "Invalid UoM Table",
            Description: null,
            DayEntries: [new NewFeedingTableDayEntryDto(1, "PreInicio", 100m, "XYZ", null, null)]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dayEntries");
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldReturnValidationError_WhenUnitOfMeasureIsEmpty()
    {
        // Arrange
        var dto = new NewFeedingTableDto(
            Name: "Empty UoM Table",
            Description: null,
            DayEntries: [new NewFeedingTableDayEntryDto(1, "PreInicio", 100m, "", null, null)]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "dayEntries");
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldPersistAmountPerBird()
    {
        // Arrange
        var dto = new NewFeedingTableDto(
            Name: "AmountPerBird Table",
            Description: null,
            DayEntries: [new NewFeedingTableDayEntryDto(1, "Inicio", 123.45m, "Gram", null, null)]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var entry = Assert.Single(result.Value!.CreatedFeedingTable.DayEntries);
        Assert.Equal(123.45m, entry.AmountPerBird);
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldPersistExpectedBirdWeight_WhenProvided()
    {
        // Arrange
        var dto = new NewFeedingTableDto(
            Name: "ExpectedWeight Table",
            Description: null,
            DayEntries: [new NewFeedingTableDayEntryDto(1, "Engorde", 200m, "Gram", 1.5m, "Kilogram")]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var entry = Assert.Single(result.Value!.CreatedFeedingTable.DayEntries);
        Assert.Equal(1.5m, entry.ExpectedBirdWeight);
        Assert.Equal("Kilogram", entry.ExpectedBirdWeightUnitOfMeasure);
    }

    [Fact]
    public async Task CreateFeedingTableCommand_ShouldAllowNullExpectedBirdWeight()
    {
        // Arrange
        var dto = new NewFeedingTableDto(
            Name: "No ExpectedWeight Table",
            Description: null,
            DayEntries: [new NewFeedingTableDayEntryDto(1, "Inicio", 100m, "Gram", null, null)]);
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var entry = Assert.Single(result.Value!.CreatedFeedingTable.DayEntries);
        Assert.Null(entry.ExpectedBirdWeight);
        Assert.Null(entry.ExpectedBirdWeightUnitOfMeasure);
    }
}

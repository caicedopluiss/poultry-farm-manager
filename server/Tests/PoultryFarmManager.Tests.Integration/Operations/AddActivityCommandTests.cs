using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Operations.Commands;
using PoultryFarmManager.Application.Operations.DTOs;
using PoultryFarmManager.Core.Operations;
using SharedLib.CQRS;

namespace PoultryFarmManager.Tests.Integration.Operations;

public class AddActivityCommandTests : IAsyncLifetime
{
    private readonly static InfrastructureContextFixture fixture = new(nameof(AddActivityCommandTests));


    private readonly IServiceProvider serviceProvider;
    private readonly IntegrationTestsDbContext dbContext;

    public AddActivityCommandTests()
    {
        serviceProvider = fixture.CreateServicesScope();
        dbContext = serviceProvider.GetRequiredService<IntegrationTestsDbContext>();
    }

    public async Task InitializeAsync()
    {
        await dbContext.ClearDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AddActivityCommand_ShouldAddActivity()
    {
        // Arrange
        var batch = dbContext.CreateBroilerBatch();
        await dbContext.BroilerBatches.AddAsync(batch);
        await dbContext.SaveChangesAsync();

        var newActivity = new NewActivityDto
        {
            BroilerBatchId = batch.Id,
            Date = DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type = ActivityType.WeightMeasurement.ToString(),
            Description = "Test activity",
            Value = 100.5m,
            Unit = "kg"
        };

        var command = new AddActivityCommand.Args(newActivity);
        var request = new AppRequest<AddActivityCommand.Args>(command);
        var handler = serviceProvider.GetRequiredService<IAppRequestHandler<AddActivityCommand.Args, AddActivityCommand.Result>>();

        // Act
        var result = await handler.HandleAsync(request, default);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, "Activity should be added successfully.");
        Assert.NotNull(result.Value);
        Assert.Equal(newActivity.BroilerBatchId, result.Value.Activity.BroilerBatchId);

        // Check if the activity is saved in the database
        var activity = await dbContext.Activities.FirstOrDefaultAsync(a => a.Id == result.Value.Activity.Id);
        Assert.NotNull(activity);
    }

    [Fact]
    public async Task AddActivityCommand_ShouldFail_WhenBatchNotFound()
    {
        // Arrange
        var nonExistentBatchId = Guid.NewGuid();
        var newActivity = new NewActivityDto
        {
            BroilerBatchId = nonExistentBatchId,
            Date = DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type = ActivityType.WeightMeasurement.ToString(),
            Description = "Test activity",
            Value = 100.5m,
            Unit = "kg"
        };

        var command = new AddActivityCommand.Args(newActivity);
        var request = new AppRequest<AddActivityCommand.Args>(command);
        var handler = serviceProvider.GetRequiredService<IAppRequestHandler<AddActivityCommand.Args, AddActivityCommand.Result>>();

        // Act
        var result = await handler.HandleAsync(request, default);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess, "Activity should not be added when batch is not found.");
        Assert.Null(result.Value);
        Assert.Contains(result.ValidationErrors, e => e.field == "broilerBatchId" && e.error == "Broiler Batch not found.");
    }

    [Fact]
    public async Task AddActivityCommand_ShouldFail_WhenInvalidDataProvided()
    {
        // Arrange
        await dbContext.SaveChangesAsync();

        var newActivity = new NewActivityDto
        {
            BroilerBatchId = Guid.Empty, // Invalid batch ID
            Date = "invalid-date", // Invalid date format
            Type = "InvalidType", // Invalid activity type
            Description = "  ",
            Value = -100.5m,
            Unit = null
        };

        var command = new AddActivityCommand.Args(newActivity);
        var request = new AppRequest<AddActivityCommand.Args>(command);
        var handler = serviceProvider.GetRequiredService<IAppRequestHandler<AddActivityCommand.Args, AddActivityCommand.Result>>();

        // Act
        var result = await handler.HandleAsync(request, default);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess, "Activity should not be added with invalid data.");
        Assert.Null(result.Value);
        Assert.Contains(result.ValidationErrors, e => e.field == "date" && e.error == "Invalid date format.");
        Assert.Contains(result.ValidationErrors, e => e.field == "type" && e.error == "Invalid activity type.");
        Assert.Contains(result.ValidationErrors, e => e.field == "broilerBatchId" && e.error == "BroilerBatchId is required.");
        Assert.Contains(result.ValidationErrors, e => e.field == "description" && e.error == "Description cannot be empty if provided.");
        Assert.Contains(result.ValidationErrors, e => e.field == "value" && e.error == "Value cannot be negative.");
    }

    [Fact]
    public async Task AddActivityCommand_ShouldFail_WhenDescriptionOrUnitTooLong()
    {
        // Arrange
        var batch = dbContext.CreateBroilerBatch();
        await dbContext.BroilerBatches.AddAsync(batch);
        await dbContext.SaveChangesAsync();

        var newActivity = new NewActivityDto
        {
            BroilerBatchId = batch.Id,
            Date = DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type = ActivityType.Cleaning.ToString(),
            Description = new string('a', 501), // 501 characters
            Value = 100.5m,
            Unit = new string('b', 11) // 11 characters
        };

        var command = new AddActivityCommand.Args(newActivity);
        var request = new AppRequest<AddActivityCommand.Args>(command);
        var handler = serviceProvider.GetRequiredService<IAppRequestHandler<AddActivityCommand.Args, AddActivityCommand.Result>>();

        // Act
        var result = await handler.HandleAsync(request, default);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess, "Activity should not be added with too long description or unit.");
        Assert.Null(result.Value);
        Assert.Contains(result.ValidationErrors, e => e.field == "description" && e.error == "Description cannot exceed 500 characters.");
        Assert.Contains(result.ValidationErrors, e => e.field == "unit" && e.error == "Unit cannot exceed 10 characters.");
    }

    [Fact]
    public async Task AddActivityCommand_ShouldFail_WhenValueNullAndUnitNotNull()
    {
        // Arrange
        var batch = dbContext.CreateBroilerBatch();
        await dbContext.BroilerBatches.AddAsync(batch);
        await dbContext.SaveChangesAsync();

        var newActivity = new NewActivityDto
        {
            BroilerBatchId = batch.Id,
            Date = DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type = ActivityType.Cleaning.ToString(),
            Description = "Test activity",
            Value = null, // Value is null
            Unit = "kg" // Unit is provided
        };

        var command = new AddActivityCommand.Args(newActivity);
        var request = new AppRequest<AddActivityCommand.Args>(command);
        var handler = serviceProvider.GetRequiredService<IAppRequestHandler<AddActivityCommand.Args, AddActivityCommand.Result>>();

        // Act
        var result = await handler.HandleAsync(request, default);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess, "Activity should not be added when Value is null and Unit is provided.");
        Assert.Null(result.Value);
        Assert.Contains(result.ValidationErrors, e => e.field == "value" && e.error == "Value is required if Unit is provided.");
    }

    [Fact]
    public async Task AddActivityCommand_ShouldFail_WhenValueNotNullAndUnitNull()
    {
        // Arrange
        var batch = dbContext.CreateBroilerBatch();
        await dbContext.BroilerBatches.AddAsync(batch);
        await dbContext.SaveChangesAsync();

        var newActivity = new NewActivityDto
        {
            BroilerBatchId = batch.Id,
            Date = DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type = ActivityType.WeightMeasurement.ToString(),
            Description = "Test activity",
            Value = 100.5m, // Value is provided
            Unit = null // Unit is not provided
        };

        var command = new AddActivityCommand.Args(newActivity);
        var request = new AppRequest<AddActivityCommand.Args>(command);
        var handler = serviceProvider.GetRequiredService<IAppRequestHandler<AddActivityCommand.Args, AddActivityCommand.Result>>();

        // Act
        var result = await handler.HandleAsync(request, default);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess, "Activity should not be added when Value is provided and Unit is not.");
        Assert.Null(result.Value);
        Assert.Contains(result.ValidationErrors, e => e.field == "unit" && e.error == "Unit is required if Value is provided.");
    }

    [Fact]
    public async Task AddActivityCommand_ShouldUpdateBatchPopulation_WhenMortalityActivity()
    {
        // Arrange
        var batch = dbContext.CreateBroilerBatch();
        batch.CurrentPopulation = 100; // Set initial population

        await dbContext.BroilerBatches.AddAsync(batch);
        await dbContext.SaveChangesAsync();

        var newActivity = new NewActivityDto
        {
            BroilerBatchId = batch.Id,
            Date = DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type = ActivityType.Mortality.ToString(),
            Description = "Test activity",
            Value = 5,
            Unit = "units"
        };

        var command = new AddActivityCommand.Args(newActivity);
        var request = new AppRequest<AddActivityCommand.Args>(command);
        var handler = serviceProvider.GetRequiredService<IAppRequestHandler<AddActivityCommand.Args, AddActivityCommand.Result>>();

        // Act
        var result = await handler.HandleAsync(request, default);

        var affectedBatch = await dbContext.BroilerBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == batch.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess, "Activity should be added successfully.");
        Assert.NotNull(result.Value);
        Assert.Equal(95, affectedBatch?.CurrentPopulation);
    }

}
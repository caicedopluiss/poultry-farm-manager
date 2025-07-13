using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Operations.Queries;
using PoultryFarmManager.Core.Operations;
using PoultryFarmManager.Core.Operations.Models;
using SharedLib.CQRS;

namespace PoultryFarmManager.Tests.Integration.Operations;

public class GetBroilerBatchByIdQueryTests : IAsyncLifetime
{
    private static readonly InfrastructureContextFixture fixture = new(nameof(GetBroilerBatchByIdQueryTests));

    private readonly IServiceProvider serviceProvider;
    private readonly IntegrationTestsDbContext dbContext;

    public GetBroilerBatchByIdQueryTests()
    {
        serviceProvider = fixture.CreateServicesScope();
        dbContext = serviceProvider.GetRequiredService<IntegrationTestsDbContext>();
    }

    public Task InitializeAsync()
    {
        // Clear the database before each test
        return dbContext.ClearDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        // Clean up any resources used by the tests
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetBroilerBatchByIdQuery_ShouldReturnBatch_WhenBatchExists()
    {
        // Arrange
        var batch = dbContext.CreateBroilerBatch();
        dbContext.BroilerBatches.Add(batch);
        await dbContext.SaveChangesAsync();

        var handler = serviceProvider.GetRequiredService<IAppRequestHandler<GetBroilerBatchByIdQuery.Args, GetBroilerBatchByIdQuery.Result>>();

        var query = new GetBroilerBatchByIdQuery.Args(batch.Id, false);
        var request = new AppRequest<GetBroilerBatchByIdQuery.Args>(query);

        // Act
        var result = await handler.HandleAsync(request, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(batch.Id, result.Value?.Batch?.Id);
        Assert.Equal(batch.BatchName, result.Value?.Batch?.BatchName);
        Assert.Equal(batch.InitialPopulation, result.Value?.Batch?.InitialPopulation);
        Assert.Equal(batch.CurrentPopulation, result.Value?.Batch?.CurrentPopulation);
        Assert.Equal(batch.StartDate, result.Value?.Batch?.StartDate);
        Assert.Equal(batch.Status.ToString(), result.Value?.Batch?.Status);
        Assert.Equal(batch.Notes, result.Value?.Batch?.Notes);

        // Financial transaction should be null if not included
        Assert.Null(result.Value?.Batch?.FinancialTransaction);

        Assert.Null(result.Value?.Batch?.LastWeightActivity);
    }

    [Fact]
    public async Task GetBroilerBatchByIdQuery_ShouldReturnNull_WhenBatchDoesNotExist()
    {
        // Arrange
        var nonExistentBatchId = Guid.NewGuid();
        var query = new GetBroilerBatchByIdQuery.Args(nonExistentBatchId, false);
        var request = new AppRequest<GetBroilerBatchByIdQuery.Args>(query);

        // Act
        var handler = serviceProvider.GetRequiredService<IAppRequestHandler<GetBroilerBatchByIdQuery.Args, GetBroilerBatchByIdQuery.Result>>();
        var result = await handler.HandleAsync(request, default);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value?.Batch);
    }

    [Fact]
    public async Task GetBroilerBatchByIdQuery_ShouldReturnBatchWithLastWeightActivity_WhenBatchExists()
    {
        // Arrange
        var batch = dbContext.CreateBroilerBatch();
        var activity = dbContext.CreateActivity(batch.Id, ActivityType.WeightMeasurement);
        dbContext.BroilerBatches.Add(batch);
        dbContext.Activities.Add(activity);
        await dbContext.SaveChangesAsync();
        // Create a query to get the batch by ID
        var query = new GetBroilerBatchByIdQuery.Args(batch.Id, false);
        var request = new AppRequest<GetBroilerBatchByIdQuery.Args>(query);

        // Act
        var handler = serviceProvider.GetRequiredService<IAppRequestHandler<GetBroilerBatchByIdQuery.Args, GetBroilerBatchByIdQuery.Result>>();
        var result = await handler.HandleAsync(request, default);
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Value?.Batch);
        Assert.Equal(batch.Id, result.Value?.Batch?.Id);

        Assert.Null(result.Value?.Batch?.FinancialTransaction);

        Assert.NotNull(result.Value?.Batch?.LastWeightActivity);
        Assert.Equal(activity.Id, result.Value?.Batch?.LastWeightActivity?.Id);
        Assert.Equal(activity.Description, result.Value?.Batch?.LastWeightActivity?.Description);
        Assert.Equal(activity.Date.ToString(Constants.DateTimeFormat), result.Value?.Batch?.LastWeightActivity?.Date);
        Assert.Equal(activity.Value, result.Value?.Batch?.LastWeightActivity?.Value);
        Assert.Equal(activity.Unit, result.Value?.Batch?.LastWeightActivity?.Unit);
        Assert.Equal(activity.Type.ToString(), result.Value?.Batch?.LastWeightActivity?.Type);
    }

    [Fact]
    public async Task GetBroilerBatchByIdQuery_ShouldReturnBatchWithFinancialTransaction_WhenIncludeFinancialTransactionIsTrue()
    {
        // Arrange
        var batch = dbContext.CreateBroilerBatch();
        dbContext.BroilerBatches.Add(batch);
        await dbContext.SaveChangesAsync();

        var query = new GetBroilerBatchByIdQuery.Args(batch.Id, true);
        var request = new AppRequest<GetBroilerBatchByIdQuery.Args>(query);

        // Act
        var handler = serviceProvider.GetRequiredService<IAppRequestHandler<GetBroilerBatchByIdQuery.Args, GetBroilerBatchByIdQuery.Result>>();
        var result = await handler.HandleAsync(request, default);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Value?.Batch);

        Assert.NotNull(result.Value?.Batch?.FinancialTransaction);
        Assert.Equal(batch.FinancialTransaction?.Id, result.Value?.Batch?.FinancialTransaction?.Id);
        Assert.Equal(batch.FinancialTransaction?.Amount, result.Value?.Batch?.FinancialTransaction?.Amount);
        Assert.Equal(batch.FinancialTransaction?.TransactionDate.ToString(Constants.DateTimeFormat), result.Value?.Batch?.FinancialTransaction?.TransactionDate);
        Assert.Equal(batch.FinancialTransaction?.Type.ToString(), result.Value?.Batch?.FinancialTransaction?.Type);
        Assert.Equal(batch.FinancialTransaction?.Category.ToString(), result.Value?.Batch?.FinancialTransaction?.Category);
        Assert.Equal(batch.FinancialTransaction?.DueDate?.ToString(Constants.DateTimeFormat), result.Value?.Batch?.FinancialTransaction?.DueDate);
        Assert.Equal(batch.FinancialTransaction?.Status.ToString(), result.Value?.Batch?.FinancialTransaction?.Status);
        Assert.Equal(batch.FinancialTransaction?.PaidAmount, result.Value?.Batch?.FinancialTransaction?.PaidAmount);
        Assert.Equal(batch.FinancialTransaction?.Notes, result.Value?.Batch?.FinancialTransaction?.Notes);

        Assert.NotNull(result.Value?.Batch?.FinancialTransaction?.FinancialEntity);
        Assert.Equal(batch.FinancialTransaction?.FinancialEntity?.Id, result.Value?.Batch?.FinancialTransaction?.FinancialEntity?.Id);
        Assert.Equal(batch.FinancialTransaction?.FinancialEntity?.Name, result.Value?.Batch?.FinancialTransaction?.FinancialEntity?.Name);
        Assert.Equal(batch.FinancialTransaction?.FinancialEntity?.Type.ToString(), result.Value?.Batch?.FinancialTransaction?.FinancialEntity?.Type);
        Assert.Equal(batch.FinancialTransaction?.FinancialEntity?.ContactPhoneNumber, result.Value?.Batch?.FinancialTransaction?.FinancialEntity?.ContactPhoneNumber);
    }

    [Fact]
    public async Task GetBroilerBatchByIdQuery_ShouldReturnBatchWithoutFinancialTransaction_WhenIncludeFinancialTransactionIsFalse()
    {
        var batch = dbContext.CreateBroilerBatch();
        dbContext.BroilerBatches.Add(batch);
        await dbContext.SaveChangesAsync();

        var query = new GetBroilerBatchByIdQuery.Args(batch.Id, false);
        var request = new AppRequest<GetBroilerBatchByIdQuery.Args>(query);
        // Act
        var handler = serviceProvider.GetRequiredService<IAppRequestHandler<GetBroilerBatchByIdQuery.Args, GetBroilerBatchByIdQuery.Result>>();
        var result = await handler.HandleAsync(request, default);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Value?.Batch);
        Assert.Equal(batch.Id, result.Value?.Batch?.Id);
        Assert.Null(result.Value?.Batch?.FinancialTransaction);
    }

    [Fact]
    public async Task GetBroilerBatchByIdQuery_ShouldReturnBatchWithLastWeightActivity_WhenBatchExistsMultipleActivities()
    {
        // Arrange
        var batch = dbContext.CreateBroilerBatch();
        dbContext.BroilerBatches.Add(batch);

        var activities = new List<Activity>
        {
            dbContext.CreateActivity(batch.Id, ActivityType.WeightMeasurement),
            dbContext.CreateActivity(batch.Id, ActivityType.WeightMeasurement),
            dbContext.CreateActivity(batch.Id, ActivityType.Feeding)
        };
        var expectedActivity = activities
            .Where(a => a.Type == ActivityType.WeightMeasurement)
            .OrderByDescending(a => a.Date)
            .FirstOrDefault();
        await dbContext.Activities.AddRangeAsync(activities);
        await dbContext.SaveChangesAsync();

        var query = new GetBroilerBatchByIdQuery.Args(batch.Id, true);
        var request = new AppRequest<GetBroilerBatchByIdQuery.Args>(query);

        // Act
        var handler = serviceProvider.GetRequiredService<IAppRequestHandler<GetBroilerBatchByIdQuery.Args, GetBroilerBatchByIdQuery.Result>>();
        var result = await handler.HandleAsync(request, default);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Value?.Batch);
        Assert.NotNull(result.Value?.Batch?.LastWeightActivity);
        Assert.Equal(expectedActivity?.Id, result.Value?.Batch?.LastWeightActivity?.Id);
        Assert.Equal(expectedActivity?.Description, result.Value?.Batch?.LastWeightActivity?.Description);
        Assert.Equal(expectedActivity?.Date.ToString(Constants.DateTimeFormat), result.Value?.Batch?.LastWeightActivity?.Date);
        Assert.Equal(expectedActivity?.Value, result.Value?.Batch?.LastWeightActivity?.Value);
        Assert.Equal(expectedActivity?.Unit, result.Value?.Batch?.LastWeightActivity?.Unit);
        Assert.Equal(expectedActivity?.Type.ToString(), result.Value?.Batch?.LastWeightActivity?.Type);
    }
}
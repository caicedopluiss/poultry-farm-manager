using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Queries.Batches;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.Core.Models.BatchActivities;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetBatchByIdQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetBatchByIdQuery.Args, GetBatchByIdQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetBatchByIdQuery.Args, GetBatchByIdQuery.Result>>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GetBatchByIdQuery_ShouldReturnNull_WhenBatchDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new AppRequest<GetBatchByIdQuery.Args>(new(nonExistentId));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value.Batch);
    }

    [Fact]
    public async Task GetBatchByIdQuery_ShouldReturnBatch_WhenBatchExists()
    {
        // Arrange - Add a batch to the database
        var batch = fixture.CreateRandomEntity<Batch>();
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetBatchByIdQuery.Args>(new(batch.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value.Batch);
        Assert.Equal(batch.Id, result.Value.Batch.Id);
        Assert.Equal(batch.Name, result.Value.Batch.Name);
        Assert.Equal(batch.Breed, result.Value.Batch.Breed);
        Assert.Equal(batch.Status.ToString(), result.Value.Batch.Status);
        Assert.Equal(batch.InitialPopulation, result.Value.Batch.InitialPopulation);
        Assert.Equal(batch.MaleCount, result.Value.Batch.MaleCount);
        Assert.Equal(batch.FemaleCount, result.Value.Batch.FemaleCount);
        Assert.Equal(batch.UnsexedCount, result.Value.Batch.UnsexedCount);
        Assert.Equal(batch.Shed, result.Value.Batch.Shed);
    }

    [Fact]
    public async Task GetBatchByIdQuery_ShouldReturnValidationError_WhenIdIsEmpty()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var request = new AppRequest<GetBatchByIdQuery.Args>(new(emptyId));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "Id" && e.error == "Id cannot be empty");
    }

    [Fact]
    public async Task GetBatchByIdQuery_ShouldReturnAllActivityTypes_WhenMultipleActivitiesExist()
    {
        // Arrange - Create a batch with all activity types
        var batch = fixture.CreateRandomEntity<Batch>();
        dbContext.Batches.Add(batch);

        var product = fixture.CreateRandomEntity<Product>();
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        // Add a mortality registration activity
        var mortalityActivity = new MortalityRegistrationBatchActivity
        {
            Id = Guid.NewGuid(),
            BatchId = batch.Id,
            Type = BatchActivityType.MortalityRecording,
            Date = DateTime.UtcNow.AddDays(-3),
            Notes = "Test mortality",
            Sex = Sex.Male,
            NumberOfDeaths = 5
        };
        dbContext.MortalityRegistrationActivities.Add(mortalityActivity);

        // Add a status switch activity
        var statusActivity = new StatusSwitchBatchActivity
        {
            Id = Guid.NewGuid(),
            BatchId = batch.Id,
            Type = BatchActivityType.StatusSwitch,
            Date = DateTime.UtcNow.AddDays(-2),
            Notes = "Test status change",
            NewStatus = BatchStatus.ForSale
        };
        dbContext.StatusSwitchActivities.Add(statusActivity);

        // Add a product consumption activity
        var consumptionActivity = new ProductConsumptionBatchActivity
        {
            Id = Guid.NewGuid(),
            BatchId = batch.Id,
            Type = BatchActivityType.ProductConsumption,
            Date = DateTime.UtcNow.AddDays(-1),
            Notes = "Test consumption",
            ProductId = product.Id,
            Stock = 10.5m,
            UnitOfMeasure = UnitOfMeasure.Kilogram
        };
        dbContext.ProductConsumptionActivities.Add(consumptionActivity);

        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetBatchByIdQuery.Args>(new(batch.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value.Batch);
        Assert.Equal(3, result.Value.Activities.Count());

        var activities = result.Value.Activities.ToList();

        // Verify we have all three activity types
        Assert.Contains(activities, a => a.Type == "MortalityRecording");
        Assert.Contains(activities, a => a.Type == "StatusSwitch");
        Assert.Contains(activities, a => a.Type == "ProductConsumption");

        // Verify mortality activity details
        var mortality = activities.FirstOrDefault(a => a.Type == "MortalityRecording") as MortalityRegistrationActivityDto;
        Assert.NotNull(mortality);
        Assert.Equal(mortalityActivity.Id, mortality.Id);
        Assert.Equal(5, mortality.NumberOfDeaths);
        Assert.Equal("Male", mortality.Sex);

        // Verify status switch activity details
        var status = activities.FirstOrDefault(a => a.Type == "StatusSwitch") as StatusSwitchActivityDto;
        Assert.NotNull(status);
        Assert.Equal(statusActivity.Id, status.Id);
        Assert.Equal("ForSale", status.NewStatus);

        // Verify product consumption activity details
        var consumption = activities.FirstOrDefault(a => a.Type == "ProductConsumption") as ProductConsumptionActivityDto;
        Assert.NotNull(consumption);
        Assert.Equal(consumptionActivity.Id, consumption.Id);
        Assert.Equal(product.Id, consumption.ProductId);
        Assert.Equal(product.Name, consumption.ProductName);
        Assert.Equal(10.5m, consumption.Stock);
        Assert.Equal("Kilogram", consumption.UnitOfMeasure);
    }
}

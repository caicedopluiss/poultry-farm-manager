using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Operations.Commands;
using PoultryFarmManager.Application.Operations.DTOs;
using PoultryFarmManager.Core.Operations.Models;
using PoultryFarmManager.Infrastructure;
using SharedLib.CQRS;

namespace PoultryFarmManager.Tests.Integration.Operations;

[Collection(InfrastructureContextCollection.Name)]
public class UpdateBroilerBatchCommandTests(InfrastructureContextFixture fixture) : IAsyncLifetime
{
    private readonly ApplicationDbContext dbContext = fixture.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    public async Task DisposeAsync()
    {
        var dbContext = fixture.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Set<BroilerBatch>().RemoveRange(dbContext.Set<BroilerBatch>());
        await dbContext.SaveChangesAsync();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task UpdateBroilerBatchCommand_ShouldSucceed_WhenValidDataProvided()
    {
        // Arrange
        var serviceProvider = fixture.ServiceProvider;
        var commandHandler = serviceProvider.GetRequiredService<IAppRequestHandler<UpdateBroilerBatchCommand.Args, UpdateBroilerBatchCommand.Result>>();

        var existingBatch = new BroilerBatch
        {
            BatchName = "Initial Batch",
            InitialPopulation = 1000,
            CurrentPopulation = 1000,
            StartDate = DateTime.UtcNow,
            Status = BroilerBatchStatus.ForSale,
            CreatedAt = DateTime.UtcNow,
            Notes = "Initial batch for testing"
        };

        existingBatch = (await dbContext.BroilerBatches.AddAsync(existingBatch)).Entity;
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateBroilerBatchDto
        {
            BatchName = "Updated Batch",
            InitialPopulation = 1000,
            CurrentPopulation = 900,
            StartClientDate = existingBatch.StartDate?.ToString(Constants.DateTimeFormat),
            ProcessingStartClientDate = existingBatch.ProcessingStartDate?.ToString(Constants.DateTimeFormat),
            ProcessingEndClientDate = existingBatch.ProcessingEndDate?.ToString(Constants.DateTimeFormat),
            Breed = existingBatch.Breed,
            Status = BroilerBatchStatus.ForSale.ToString(),
            Notes = "Updated notes"
        };

        var request = new AppRequest<UpdateBroilerBatchCommand.Args>(new(existingBatch.Id, updateDto));

        // Act
        var result = await commandHandler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        Assert.Equal(existingBatch.Id, result.Value.BatchDto.Id);
        Assert.Equal(updateDto.BatchName, result.Value.BatchDto.BatchName);
        Assert.Equal(updateDto.Status, result.Value.BatchDto.Status);
        Assert.Equal(updateDto.Notes, result.Value.BatchDto.Notes);
        Assert.Equal(updateDto.CurrentPopulation, result.Value.BatchDto.CurrentPopulation);
        Assert.Equal(existingBatch.InitialPopulation, result.Value.BatchDto.InitialPopulation);
        Assert.Equal(existingBatch.StartDate?.ToString(Constants.DateTimeFormat), result.Value.BatchDto.StartDate?.ToString(Constants.DateTimeFormat));
        Assert.Equal(existingBatch.CreatedAt.ToString(Constants.DateTimeFormat), result.Value.BatchDto.CreatedAt.ToString(Constants.DateTimeFormat));
        Assert.Equal(existingBatch.ProcessingStartDate?.ToString(Constants.DateTimeFormat), result.Value.BatchDto.ProcessingStartDate?.ToString(Constants.DateTimeFormat));
        Assert.Equal(existingBatch.ProcessingEndDate?.ToString(Constants.DateTimeFormat), result.Value.BatchDto.ProcessingEndDate?.ToString(Constants.DateTimeFormat));
        Assert.Equal(existingBatch.Breed, result.Value.BatchDto.Breed);
    }
}
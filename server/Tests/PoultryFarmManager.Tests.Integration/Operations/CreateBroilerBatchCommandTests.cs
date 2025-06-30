using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Operations.Commands;
using PoultryFarmManager.Application.Operations.DTOs;
using PoultryFarmManager.Core.Operations.Models;
using PoultryFarmManager.Infrastructure;
using SharedLib.CQRS;

namespace PoultryFarmManager.Tests.Integration.Operations;

[Collection(InfrastructureContextCollection.Name)]
public class CreateBroilerBatchCommandTests(InfrastructureContextFixture fixture) : IAsyncLifetime
{
    public async Task DisposeAsync()
    {
        var dbContext = fixture.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.BroilerBatches.RemoveRange(dbContext.BroilerBatches);
        await dbContext.SaveChangesAsync();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateBroilerBatchCommand_ShouldSucceed_WhenValidDataProvided()
    {
        // Arrange
        var serviceProvider = fixture.ServiceProvider;
        var commandHandler = serviceProvider.GetRequiredService<IAppRequestHandler<CreateBroilerBatchCommand.Args, CreateBroilerBatchCommand.Result>>();

        var newBatchDto = new NewBroilerBatchDto
        {
            BatchName = "Test Batch",
            InitialPopulation = 1000,
            StartClientDate = "2023-10-01T00:00:00Z",
            Status = "Active",
            Notes = "Initial batch for testing"
        };

        var request = new AppRequest<CreateBroilerBatchCommand.Args>(new(newBatchDto));

        // Act
        var result = await commandHandler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value.BatchDto);
        Assert.NotEqual(default, result.Value.BatchDto.Id);
        Assert.Equal(newBatchDto.BatchName, result.Value.BatchDto.BatchName);
        Assert.Equal(newBatchDto.InitialPopulation, result.Value.BatchDto.CurrentPopulation);
    }

    [Fact]
    public async Task CreateBroilerBatchCommand_ShouldFail_WhenInvalidDataProvided()
    {
        // Arrange
        var serviceProvider = fixture.ServiceProvider;
        var commandHandler = serviceProvider.GetRequiredService<IAppRequestHandler<CreateBroilerBatchCommand.Args, CreateBroilerBatchCommand.Result>>();

        var newBatchDto = new NewBroilerBatchDto
        {
            BatchName = "", // Invalid: empty name
            InitialPopulation = -100, // Invalid: negative population
            StartClientDate = "invalid-date", // Invalid: non-ISO date
            Status = "UnknownStatus", // Invalid: unknown status
            Notes = new string('a', 600) // Invalid: too long notes
        };

        var request = new AppRequest<CreateBroilerBatchCommand.Args>(new(newBatchDto));

        // Act & Assert
        var result = await commandHandler.HandleAsync(request, CancellationToken.None);
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchName" && e.error == "Batch name cannot be empty.");
        Assert.Contains(result.ValidationErrors, e => e.field == "initialPopulation" && e.error == "Initial population must be greater than zero.");
        Assert.Contains(result.ValidationErrors, e => e.field == "startClientDate" && e.error == "Invalid ISO 8601 date format.");
        Assert.Contains(result.ValidationErrors, e => e.field == "status" && e.error == "Invalid status value.");
        Assert.Contains(result.ValidationErrors, e => e.field == "notes" && e.error == "Notes cannot exceed 500 characters.");

        // Ensure no batch was created
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var batchesCount = await dbContext.Set<BroilerBatch>().CountAsync();
        Assert.Equal(0, batchesCount);
    }
}
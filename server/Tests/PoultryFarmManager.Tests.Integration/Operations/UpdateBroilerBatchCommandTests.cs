using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Operations.Commands;
using PoultryFarmManager.Application.Operations.DTOs;
using PoultryFarmManager.Core.Finances;
using PoultryFarmManager.Core.Finances.Models;
using PoultryFarmManager.Core.Operations.Models;
using SharedLib.CQRS;

namespace PoultryFarmManager.Tests.Integration.Operations;

public class UpdateBroilerBatchCommandTests : IAsyncLifetime
{
    private static readonly InfrastructureContextFixture fixture = new InfrastructureContextFixture(nameof(UpdateBroilerBatchCommandTests));

    private readonly IntegrationTestsDbContext dbContext;

    public UpdateBroilerBatchCommandTests()
    {
        dbContext = fixture.ServiceProvider.GetRequiredService<IntegrationTestsDbContext>();
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
            Notes = "Initial batch for testing",
            FinancialTransaction = new FinancialTransaction
            {
                Amount = 5000,
                PaidAmount = 5000,
                TransactionDate = DateTime.UtcNow,
                Type = FinancialTransactionType.Expense,
                Category = FinancialTransactionCategory.LivestockPurchase,
                Status = PaymentStatus.Paid,
                Notes = "Initial purchase for batch",
                FinancialEntity = new FinancialEntity
                {
                    Name = "Test Supplier Update",
                    Type = FinancialEntityType.Supplier,
                    CreatedAt = DateTime.UtcNow
                }
            }
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
        Assert.NotEqual(default, result.Value.BatchDto.ModifiedAt);
        Assert.True(result.Value.BatchDto.ModifiedAt > existingBatch.CreatedAt, "ModifiedAt should be greater than CreatedAt");
        Assert.Equal(existingBatch.FinancialTransactionId, result.Value.BatchDto.FinancialTransaction?.Id);
    }
}
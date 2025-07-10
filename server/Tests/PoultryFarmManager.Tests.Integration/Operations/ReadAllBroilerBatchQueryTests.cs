using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Operations.Queries;
using PoultryFarmManager.Core.Finances;
using PoultryFarmManager.Core.Finances.Models;
using PoultryFarmManager.Core.Operations.Models;
using SharedLib.CQRS;

namespace PoultryFarmManager.Tests.Integration.Operations;

public class ReadAllBroilerBatchQueryTests : IAsyncLifetime
{

    private static readonly InfrastructureContextFixture fixture = new InfrastructureContextFixture(nameof(ReadAllBroilerBatchQueryTests));

    private readonly IntegrationTestsDbContext dbContext;

    public ReadAllBroilerBatchQueryTests()
    {
        dbContext = fixture.ServiceProvider.GetRequiredService<IntegrationTestsDbContext>();
    }

    public async Task InitializeAsync()
    {
        await dbContext.ClearDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ReadAllBroilerBatchQuery_ShouldReturnAllBatches()
    {
        // Arrange
        var existingBatches = new[]
        {
            new BroilerBatch
            {
                BatchName = "Batch 1",
                InitialPopulation = 1000,
                CurrentPopulation = 1000,
                StartDate = DateTime.UtcNow,
                Status = BroilerBatchStatus.Active,
                CreatedAt = DateTime.UtcNow,
                Notes = "First batch for testing",
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
                        Name = "Test Supplier",
                        Type = FinancialEntityType.Supplier,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            },
            new BroilerBatch
            {
                BatchName = "Batch 2",
                InitialPopulation = 2000,
                CurrentPopulation = 2000,
                StartDate = DateTime.UtcNow.AddDays(-10),
                Status = BroilerBatchStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                Notes = "Second batch for testing",
                FinancialTransaction = new FinancialTransaction
                {
                    Amount = 10000,
                    PaidAmount = 10000,
                    TransactionDate = DateTime.UtcNow.AddDays(-10),
                    Type = FinancialTransactionType.Expense,
                    Category = FinancialTransactionCategory.LivestockPurchase,
                    Status = PaymentStatus.Paid,
                    Notes = "Initial purchase for batch",
                    FinancialEntity = new FinancialEntity
                    {
                        Name = "Test Supplier 2",
                        Type = FinancialEntityType.Supplier,
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    }
                }
            }
        };

        await dbContext.BroilerBatches.AddRangeAsync(existingBatches);
        await dbContext.SaveChangesAsync();

        // Arrange
        dbContext.ChangeTracker.Clear();

        var serviceProvider = fixture.ServiceProvider;
        var queryHandler = serviceProvider.GetRequiredService<IAppRequestHandler<ReadAllBroilerBatchQuery.Args, ReadAllBroilerBatchQuery.Result>>();

        // Act
        var result = await queryHandler.HandleAsync(new AppRequest<ReadAllBroilerBatchQuery.Args>(new()), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value.Batches);
        Assert.Equal(existingBatches.Length, result.Value.Batches.Count());
        Assert.Contains(result.Value.Batches, b => b.BatchName == "Batch 1");
        Assert.Contains(result.Value.Batches, b => b.BatchName == "Batch 2");
        Assert.All(result.Value.Batches, b =>
        {
            Assert.NotEqual(Guid.Empty, b.Id);
            Assert.NotNull(b.BatchName);
            Assert.NotNull(b.Status);
            Assert.NotNull(b.FinancialTransaction);
            Assert.NotNull(b.FinancialTransaction.FinancialEntity);
        });
    }
}
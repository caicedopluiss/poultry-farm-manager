using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Application;
using PoultryFarmManager.Core.Finances;
using PoultryFarmManager.Core.Finances.Models;
using PoultryFarmManager.Core.Operations;
using PoultryFarmManager.Core.Operations.Models;
using PoultryFarmManager.Infrastructure;

namespace PoultryFarmManager.Tests.Integration;

public class IntegrationTestsDbContext(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
{
    internal Task ClearDatabaseAsync()
    {
        BroilerBatches.RemoveRange(BroilerBatches);
        FinancialTransactions.RemoveRange(FinancialTransactions);
        FinancialEntities.RemoveRange(FinancialEntities);
        Activities.RemoveRange(Activities);
        // Add other DbSet clearings as needed

        return SaveChangesAsync();
    }

    internal BroilerBatch CreateBroilerBatch(FinancialTransaction? financialTransaction = null, FinancialEntity? financialEntity = null)
    {
        var random = new Random();

        var batch = new BroilerBatch
        {
            Id = Guid.NewGuid(),
            BatchName = $"Test Batch {Guid.NewGuid().ToString().Substring(0, 8)}",
            InitialPopulation = random.Next(500, 2000),
            CurrentPopulation = random.Next(400, 2000),
            StartDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
            Status = BroilerBatchStatus.Active,
            Notes = $"Test notes {Guid.NewGuid().ToString().Substring(0, 4)}",
            Breed = $"Test Breed {random.Next(1, 10)}",
            ProcessingStartDate = DateTime.UtcNow.AddDays(random.Next(10, 20)),
            ProcessingEndDate = DateTime.UtcNow.AddDays(random.Next(21, 40)),
            FinancialTransaction = financialTransaction ?? new FinancialTransaction
            {
                Id = Guid.NewGuid(),
                Amount = Utils.TruncateToTwoDecimals((decimal)(random.NextDouble() * 5000 + 500)),
                TransactionDate = DateTime.UtcNow.AddDays(-random.Next(1, 10)),
                Type = FinancialTransactionType.Income,
                Category = FinancialTransactionCategory.LivestockPurchase,
                DueDate = DateTime.UtcNow.AddDays(random.Next(10, 40)),
                PaidAmount = random.NextDouble() < 0.5
                    ? Utils.TruncateToTwoDecimals((decimal)(random.NextDouble() * 5000 + 500))
                    : null,
                Notes = $"Test financial transaction {Guid.NewGuid().ToString().Substring(0, 4)}",
                FinancialEntity = financialEntity ?? new FinancialEntity
                {
                    Id = Guid.NewGuid(),
                    Name = $"Test Entity {random.Next(1, 1000)}",
                    Type = FinancialEntityType.Supplier,
                    ContactPhoneNumber = $"{random.Next(100, 999)}-{random.Next(100, 999)}-{random.Next(1000, 9999)}",
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 100)),
                    ModifiedAt = DateTime.UtcNow
                },
            }
        };

        if (financialEntity is not null) batch.FinancialTransaction.FinancialEntity = financialEntity;
        if (financialTransaction is not null) batch.FinancialTransaction = financialTransaction;

        return batch;
    }

    internal Activity CreateActivity(Guid batchId,
        ActivityType? type = null,
        string? description = null,
        decimal? value = null,
        string? unit = null)
    {
        var random = new Random();

        var nullableValue = value ?? (random.Next(0, 2) == 0 ? null : Utils.TruncateToTwoDecimals((decimal)(random.NextDouble() * 100)));
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            BroilerBatchId = batchId,
            Type = type ?? (ActivityType)random.Next(Enum.GetValues(typeof(ActivityType)).Length),
            Description = description ?? $"Test activity {Guid.NewGuid().ToString().Substring(0, 4)}",
            Date = DateTime.UtcNow.AddDays(-random.Next(0, 30)),
            Value = nullableValue,
            Unit = nullableValue is null ? null : (unit ?? (random.Next(0, 2) == 0 ? "kg" : "g")),
        };

        return activity;
    }
}

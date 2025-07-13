using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Finances.DTOs;
using PoultryFarmManager.Application.Operations.Commands;
using PoultryFarmManager.Application.Operations.DTOs;
using PoultryFarmManager.Core.Finances;
using PoultryFarmManager.Core.Finances.Models;
using PoultryFarmManager.Core.Operations.Models;
using SharedLib.CQRS;

namespace PoultryFarmManager.Tests.Integration.Operations;

public class CreateBroilerBatchCommandTests : IAsyncLifetime
{
    private static readonly InfrastructureContextFixture fixture = new(nameof(CreateBroilerBatchCommandTests));

    private readonly IServiceProvider serviceProvider;
    private readonly IntegrationTestsDbContext dbContext;

    public CreateBroilerBatchCommandTests()
    {
        serviceProvider = fixture.CreateServicesScope();
        dbContext = serviceProvider.GetRequiredService<IntegrationTestsDbContext>();
    }

    public async Task InitializeAsync()
    {
        await dbContext.ClearDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateBroilerBatchCommand_ShouldSucceed_WhenValidDataProvided()
    {
        // Arrange
        var commandHandler = serviceProvider.GetRequiredService<IAppRequestHandler<CreateBroilerBatchCommand.Args, CreateBroilerBatchCommand.Result>>();

        var newBatchDto = new NewBroilerBatchDto
        {
            BatchName = "Test Batch",
            InitialPopulation = 1000,
            StartClientDate = "2023-10-01T00:00:00Z",
            Status = "Active",
            Notes = "Initial batch for testing"
        };

        var financialTransactionDto = new NewFinancialTransactionDto
        {
            Amount = 5000,
            PaidAmount = 5000,
            TransactionClientDateDate = "2023-10-01T00:00:00Z",
            Type = nameof(FinancialTransactionType.Expense),
            Category = nameof(FinancialTransactionCategory.LivestockPurchase),
            Status = nameof(PaymentStatus.Paid),
            Notes = "Initial purchase for batch",
            // FinancialEntityId = null, // No existing entity, will create a new one
            FinancialEntityInfo = new NewFinancialEntityDto
            {
                Name = "Test Financial Entity",
                Type = nameof(FinancialEntityType.Supplier),
            }
        };

        var request = new AppRequest<CreateBroilerBatchCommand.Args>(new(newBatchDto, financialTransactionDto));

        // Act
        var result = await commandHandler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value.BatchDto);
        Assert.NotEqual(default, result.Value.BatchDto.Id);
        Assert.Equal(newBatchDto.BatchName, result.Value.BatchDto.BatchName);
        Assert.Equal(newBatchDto.InitialPopulation, result.Value.BatchDto.CurrentPopulation);

        Assert.NotEqual(default, result.Value.BatchDto.FinancialTransaction?.Id);
        Assert.Equal(financialTransactionDto.Amount, result.Value.BatchDto.FinancialTransaction?.Amount);
        Assert.Equal(financialTransactionDto.PaidAmount, result.Value.BatchDto.FinancialTransaction?.PaidAmount);
        Assert.Equal(financialTransactionDto.TransactionClientDateDate, result.Value.BatchDto.FinancialTransaction?.TransactionDate);
        Assert.Equal(financialTransactionDto.Type, result.Value.BatchDto.FinancialTransaction?.Type);
        Assert.Equal(financialTransactionDto.Category, result.Value.BatchDto.FinancialTransaction?.Category);
        Assert.Equal(financialTransactionDto.Status, result.Value.BatchDto.FinancialTransaction?.Status);
        Assert.Equal(financialTransactionDto.Notes, result.Value.BatchDto.FinancialTransaction?.Notes);

        Assert.NotNull(result.Value.BatchDto.FinancialTransaction?.FinancialEntity);
        Assert.NotEqual(default, result.Value.BatchDto.FinancialTransaction?.FinancialEntity?.Id);
        Assert.Equal(financialTransactionDto.FinancialEntityInfo.Name, result.Value.BatchDto.FinancialTransaction?.FinancialEntity?.Name);
        Assert.Equal(financialTransactionDto.FinancialEntityInfo.Type, result.Value.BatchDto.FinancialTransaction?.FinancialEntity?.Type);
        Assert.Equal(financialTransactionDto.FinancialEntityInfo.ContactPhoneNumber, result.Value.BatchDto.FinancialTransaction?.FinancialEntity?.ContactPhoneNumber);

        // Ensure the batch was created in the database
        var batchesCount = await dbContext.Set<BroilerBatch>().CountAsync();
        Assert.Equal(1, batchesCount);

        // Ensure the financial transaction was created in the database
        var financialTransactionsCount = await dbContext.Set<FinancialTransaction>().CountAsync();
        Assert.Equal(1, financialTransactionsCount);

        // Ensure the financial entity was created in the database
        var financialEntitiesCount = await dbContext.Set<FinancialEntity>().CountAsync();
        Assert.Equal(1, financialEntitiesCount);
    }

    [Fact]
    public async Task CreateBroilerBatchCommand_ShouldFail_WhenInvalidDataProvided()
    {
        // Arrange
        var commandHandler = serviceProvider.GetRequiredService<IAppRequestHandler<CreateBroilerBatchCommand.Args, CreateBroilerBatchCommand.Result>>();

        var newBatchDto = new NewBroilerBatchDto
        {
            BatchName = "", // Invalid: empty name
            InitialPopulation = -100, // Invalid: negative population
            StartClientDate = "invalid-date", // Invalid: non-ISO date
            Status = "UnknownStatus", // Invalid: unknown status
            Notes = new string('a', 600) // Invalid: too long notes
        };

        var financialTransactionDto = new NewFinancialTransactionDto
        {
            Amount = -5000, // Invalid: negative amount
            PaidAmount = 6000, // Invalid: paid amount exceeds total
            TransactionClientDateDate = "invalid-date", // Invalid: non-ISO date
            DueClientDate = "invalid-date", // Invalid: non-ISO date
            Type = "UnknownType", // Invalid: unknown type
            Category = "UnknownCategory", // Invalid: unknown category
            Status = "UnknownStatus", // Invalid: unknown status
            Notes = new string('b', 600), // Invalid: too long notes
            FinancialEntityId = null, // No existing entity, will create a new one
            FinancialEntityInfo = null // No financial entity info provided
        };

        var request = new AppRequest<CreateBroilerBatchCommand.Args>(new(newBatchDto, financialTransactionDto));

        // Act & Assert
        var result = await commandHandler.HandleAsync(request, CancellationToken.None);
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchInfo.batchName" && e.error == "Batch name cannot be empty.");
        Assert.Contains(result.ValidationErrors, e => e.field == "batchInfo.initialPopulation" && e.error == "Initial population must be greater than zero.");
        Assert.Contains(result.ValidationErrors, e => e.field == "batchInfo.startClientDate" && e.error == "Invalid ISO 8601 date format.");
        Assert.Contains(result.ValidationErrors, e => e.field == "batchInfo.status" && e.error == "Invalid status value.");
        Assert.Contains(result.ValidationErrors, e => e.field == "batchInfo.notes" && e.error == "Notes cannot exceed 500 characters.");
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.amount" && e.error == "Financial transaction amount must be greater than zero.");
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.paidAmount" && e.error == "Paid amount cannot exceed the total amount.");
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.type" && e.error == "Invalid financial transaction type.");
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.category" && e.error == "Invalid financial transaction category.");
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.status" && e.error == "Invalid financial transaction status.");
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.dueClientDate" && e.error == "Invalid ISO 8601 date format.");
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.notes" && e.error == "Notes cannot exceed 500 characters.");
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.financialEntityId" && e.error == "Financial entity ID or financial entity info must be provided.");

        // Ensure no batch was created
        var batchesCount = await dbContext.Set<BroilerBatch>().CountAsync();
        Assert.Equal(0, batchesCount);

        // Ensure no financial transaction was created
        var financialTransactionsCount = await dbContext.Set<FinancialTransaction>().CountAsync();
        Assert.Equal(0, financialTransactionsCount);

        // Ensure no financial entity was created
        var financialEntitiesCount = await dbContext.Set<FinancialEntity>().CountAsync();
        Assert.Equal(0, financialEntitiesCount);
    }

    [Fact]
    public async Task CreateBroilerBatchCommand_ShouldFail_WhenBothNewAndExistingFinancialEntityProvided()
    {
        // Arrange
        var commandHandler = serviceProvider.GetRequiredService<IAppRequestHandler<CreateBroilerBatchCommand.Args, CreateBroilerBatchCommand.Result>>();

        var newBatchDto = new NewBroilerBatchDto
        {
            BatchName = "Test Batch",
            InitialPopulation = 1000,
            StartClientDate = "2023-10-01T00:00:00Z",
            Status = "Active",
            Notes = "Initial batch for testing",
            Breed = "Ross 308",
        };

        var financialTransactionDto = new NewFinancialTransactionDto
        {
            Amount = 5000,
            PaidAmount = 5000,
            TransactionClientDateDate = "2023-10-01T00:00:00Z",
            Type = nameof(FinancialTransactionType.Expense),
            Category = nameof(FinancialTransactionCategory.LivestockPurchase),
            Status = nameof(PaymentStatus.Paid),
            Notes = "Initial purchase for batch",
            FinancialEntityId = Guid.NewGuid(), // Existing entity ID
            FinancialEntityInfo = new NewFinancialEntityDto // New entity info
            {
                Name = "Test Financial Entity",
                Type = nameof(FinancialEntityType.Supplier),
            }
        };

        var request = new AppRequest<CreateBroilerBatchCommand.Args>(new(newBatchDto, financialTransactionDto));

        // Act & Assert
        var result = await commandHandler.HandleAsync(request, CancellationToken.None);
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.financialEntityId" && e.error == "Cannot provide both financial entity ID and financial entity info.");

        // Ensure no batch was created
        var batchesCount = await dbContext.Set<BroilerBatch>().CountAsync();
        Assert.Equal(0, batchesCount);

        // Ensure no financial transaction was created
        var financialTransactionsCount = await dbContext.Set<FinancialTransaction>().CountAsync();
        Assert.Equal(0, financialTransactionsCount);

        // Ensure no financial entity was created
        var financialEntitiesCount = await dbContext.Set<FinancialEntity>().CountAsync();
        Assert.Equal(0, financialEntitiesCount);
    }


    [Fact]
    public async Task CreateBroilerBatchCommand_ShouldFail_WhenFinancialEntityIdIsEmptyGuid()
    {
        // Arrange
        var commandHandler = serviceProvider.GetRequiredService<IAppRequestHandler<CreateBroilerBatchCommand.Args, CreateBroilerBatchCommand.Result>>();

        var newBatchDto = new NewBroilerBatchDto
        {
            BatchName = "Test Batch",
            InitialPopulation = 1000,
            StartClientDate = "2023-10-01T00:00:00Z",
            Status = "Active",
            Notes = "Initial batch for testing"
        };

        var financialTransactionDto = new NewFinancialTransactionDto
        {
            Amount = 5000,
            PaidAmount = 5000,
            TransactionClientDateDate = "2023-10-01T00:00:00Z",
            Type = nameof(FinancialTransactionType.Expense),
            Category = nameof(FinancialTransactionCategory.LivestockPurchase),
            Status = nameof(PaymentStatus.Paid),
            Notes = "Initial purchase for batch",
            FinancialEntityId = Guid.Empty // Invalid: empty GUID
        };

        var request = new AppRequest<CreateBroilerBatchCommand.Args>(new(newBatchDto, financialTransactionDto));

        // Act & Assert
        var result = await commandHandler.HandleAsync(request, CancellationToken.None);
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.financialEntityId" && e.error == "Financial entity ID cannot be empty.");

        // Ensure no batch was created
        var batchesCount = await dbContext.Set<BroilerBatch>().CountAsync();
        Assert.Equal(0, batchesCount);

        // Ensure no financial transaction was created
        var financialTransactionsCount = await dbContext.Set<FinancialTransaction>().CountAsync();
        Assert.Equal(0, financialTransactionsCount);

        // Ensure no financial entity was created
        var financialEntitiesCount = await dbContext.Set<FinancialEntity>().CountAsync();
        Assert.Equal(0, financialEntitiesCount);
    }

    [Fact]
    public async Task CreateBroilerBatchCommand_ShouldFail_WhenFinancialEntityIdDoesNotExist()
    {
        // Arrange
        var commandHandler = serviceProvider.GetRequiredService<IAppRequestHandler<CreateBroilerBatchCommand.Args, CreateBroilerBatchCommand.Result>>();

        var newBatchDto = new NewBroilerBatchDto
        {
            BatchName = "Test Batch",
            InitialPopulation = 1000,
            StartClientDate = "2023-10-01T00:00:00Z",
            Status = "Active",
            Notes = "Initial batch for testing"
        };

        var financialTransactionDto = new NewFinancialTransactionDto
        {
            Amount = 5000,
            PaidAmount = 5000,
            TransactionClientDateDate = "2023-10-01T00:00:00Z",
            Type = nameof(FinancialTransactionType.Expense),
            Category = nameof(FinancialTransactionCategory.LivestockPurchase),
            Status = nameof(PaymentStatus.Paid),
            Notes = "Initial purchase for batch",
            FinancialEntityId = Guid.NewGuid() // Non-existing entity ID
        };

        var request = new AppRequest<CreateBroilerBatchCommand.Args>(new(newBatchDto, financialTransactionDto));

        // Act & Assert
        var result = await commandHandler.HandleAsync(request, CancellationToken.None);
        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.financialEntityId" && e.error == "Financial entity with the provided ID does not exist.");

        // Ensure no batch was created
        var batchesCount = await dbContext.Set<BroilerBatch>().CountAsync();
        Assert.Equal(0, batchesCount);

        // Ensure no financial transaction was created
        var financialTransactionsCount = await dbContext.Set<FinancialTransaction>().CountAsync();
        Assert.Equal(0, financialTransactionsCount);

        // Ensure no financial entity was created
        var financialEntitiesCount = await dbContext.Set<FinancialEntity>().CountAsync();
        Assert.Equal(0, financialEntitiesCount);
    }

    [Fact]
    public async Task ShouldFail_WhenFinancialEntityInfoIsInvalid()
    {
        var commandHandler = serviceProvider.GetRequiredService<IAppRequestHandler<CreateBroilerBatchCommand.Args, CreateBroilerBatchCommand.Result>>();

        var newBatchDto = new NewBroilerBatchDto
        {
            BatchName = "Batch with Invalid Financial Entity",
            InitialPopulation = 100,
            StartClientDate = "2023-10-01T00:00:00Z",
            Status = "Active",
            Notes = "Testing invalid financial entity"
        };

        var financialTransactionDto = new NewFinancialTransactionDto
        {
            Amount = 1000,
            PaidAmount = 1000,
            TransactionClientDateDate = "2023-10-01T00:00:00Z",
            Type = nameof(FinancialTransactionType.Expense),
            Category = nameof(FinancialTransactionCategory.LivestockPurchase),
            Status = nameof(PaymentStatus.Paid),
            Notes = "Transaction with invalid financial entity",
            FinancialEntityInfo = new NewFinancialEntityDto
            {
                Name = "", // Invalid: empty name
                Type = "UnknownType" // Invalid: unknown type
            }
        };

        var request = new AppRequest<CreateBroilerBatchCommand.Args>(new(newBatchDto, financialTransactionDto));

        var result = await commandHandler.HandleAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.financialEntityInfo.name" && e.error == "Financial entity name cannot be empty.");
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.financialEntityInfo.type" && e.error == "Invalid financial entity type.");

        // Ensure no batch was created
        var batchesCount = await dbContext.Set<BroilerBatch>().CountAsync();
        Assert.Equal(0, batchesCount);

        // Ensure no financial transaction was created
        var financialTransactionsCount = await dbContext.Set<FinancialTransaction>().CountAsync();
        Assert.Equal(0, financialTransactionsCount);

        // Ensure no financial entity was created
        var financialEntitiesCount = await dbContext.Set<FinancialEntity>().CountAsync();
        Assert.Equal(0, financialEntitiesCount);
    }

    [Fact]
    public async Task ShouldFail_WhenFinancialEntityInfoNameIsTooLong()
    {
        var commandHandler = serviceProvider.GetRequiredService<IAppRequestHandler<CreateBroilerBatchCommand.Args, CreateBroilerBatchCommand.Result>>();

        var newBatchDto = new NewBroilerBatchDto
        {
            BatchName = "Batch with Long Financial Entity Name",
            InitialPopulation = 100,
            StartClientDate = "2023-10-01T00:00:00Z",
            Status = "Active",
            Notes = "Testing long financial entity name"
        };

        var financialTransactionDto = new NewFinancialTransactionDto
        {
            Amount = 1000,
            PaidAmount = 1000,
            TransactionClientDateDate = "2023-10-01T00:00:00Z",
            Type = nameof(FinancialTransactionType.Expense),
            Category = nameof(FinancialTransactionCategory.LivestockPurchase),
            Status = nameof(PaymentStatus.Paid),
            Notes = "Transaction with long financial entity name",
            FinancialEntityInfo = new NewFinancialEntityDto
            {
                Name = new string('x', 201), // Invalid: name too long (assuming max 200)
                Type = nameof(FinancialEntityType.Supplier)
            }
        };

        var request = new AppRequest<CreateBroilerBatchCommand.Args>(new(newBatchDto, financialTransactionDto));

        var result = await commandHandler.HandleAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.financialEntityInfo.name" && e.error == "Financial entity name cannot exceed 100 characters.");

        // Ensure no batch was created
        var batchesCount = await dbContext.Set<BroilerBatch>().CountAsync();
        Assert.Equal(0, batchesCount);

        // Ensure no financial transaction was created
        var financialTransactionsCount = await dbContext.Set<FinancialTransaction>().CountAsync();
        Assert.Equal(0, financialTransactionsCount);

        // Ensure no financial entity was created
        var financialEntitiesCount = await dbContext.Set<FinancialEntity>().CountAsync();
        Assert.Equal(0, financialEntitiesCount);
    }

    [Fact]
    public async Task ShouldFail_WhenFinancialEntityPhoneNumberIsInvalid()
    {
        var commandHandler = serviceProvider.GetRequiredService<IAppRequestHandler<CreateBroilerBatchCommand.Args, CreateBroilerBatchCommand.Result>>();

        var newBatchDto = new NewBroilerBatchDto
        {
            BatchName = "Batch with Invalid Phone Number",
            InitialPopulation = 100,
            StartClientDate = "2023-10-01T00:00:00Z",
            Status = "Active",
            Notes = "Testing invalid phone number"
        };

        var financialTransactionDto = new NewFinancialTransactionDto
        {
            Amount = 1000,
            PaidAmount = 1000,
            TransactionClientDateDate = "2023-10-01T00:00:00Z",
            Type = nameof(FinancialTransactionType.Expense),
            Category = nameof(FinancialTransactionCategory.LivestockPurchase),
            Status = nameof(PaymentStatus.Paid),
            Notes = "Transaction with invalid phone number",
            FinancialEntityInfo = new NewFinancialEntityDto
            {
                Name = "Test Entity",
                Type = nameof(FinancialEntityType.Supplier),
                ContactPhoneNumber = "1234567890123456" // Invalid: too long
            }
        };

        var request = new AppRequest<CreateBroilerBatchCommand.Args>(new(newBatchDto, financialTransactionDto));

        var result = await commandHandler.HandleAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.Value);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "financialTransactionInfo.financialEntityInfo.contactPhoneNumber" && e.error == "Financial entity contact phone number cannot exceed 15 characters.");

        // Ensure no batch was created
        var batchesCount = await dbContext.Set<BroilerBatch>().CountAsync();
        Assert.Equal(0, batchesCount);

        // Ensure no financial transaction was created
        var financialTransactionsCount = await dbContext.Set<FinancialTransaction>().CountAsync();
        Assert.Equal(0, financialTransactionsCount);

        // Ensure no financial entity was created
        var financialEntitiesCount = await dbContext.Set<FinancialEntity>().CountAsync();
        Assert.Equal(0, financialEntitiesCount);
    }

}
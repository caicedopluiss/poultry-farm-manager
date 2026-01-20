using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Commands.Transactions;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class CreateTransactionCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<CreateTransactionCommand.Args, CreateTransactionCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<CreateTransactionCommand.Args, CreateTransactionCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateTransactionCommand_ShouldCreateIncomeTransaction_WithAllFields()
    {
        // Arrange - Create dependencies
        var customer = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "555-0100"
        };
        dbContext.Persons.Add(customer);

        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Transaction",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed A-1"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var newTransaction = new NewTransactionDto(
            Title: "Sale of 50 chickens",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Income",
            UnitPrice: 15.50m,
            Quantity: 50,
            TransactionAmount: 775.00m,
            Notes: "Sold to local market",
            ProductVariantId: null,
            BatchId: batch.Id,
            VendorId: null,
            CustomerId: customer.Id
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        var createdTransaction = await dbContext.Transactions.FindAsync(result.Value!.CreatedTransaction.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.CreatedTransaction.Id);
        Assert.Equal("Sale of 50 chickens", result.Value!.CreatedTransaction.Title);
        Assert.Equal("Income", result.Value!.CreatedTransaction.Type);
        Assert.Equal(15.50m, result.Value!.CreatedTransaction.UnitPrice);
        Assert.Equal(50, result.Value!.CreatedTransaction.Quantity);
        Assert.Equal(775.00m, result.Value!.CreatedTransaction.TransactionAmount);
        Assert.Equal(775.00m, result.Value!.CreatedTransaction.TotalAmount); // (50 * 15.50)
        Assert.Equal("Sold to local market", result.Value!.CreatedTransaction.Notes);
        Assert.Equal(batch.Id, result.Value!.CreatedTransaction.BatchId);
        Assert.Equal(customer.Id, result.Value!.CreatedTransaction.CustomerId);

        // Verify database record
        Assert.NotNull(createdTransaction);
        Assert.Equal(createdTransaction!.Id, result.Value!.CreatedTransaction.Id);
        Assert.Equal(TransactionType.Income, createdTransaction.Type);
    }

    [Fact]
    public async Task CreateTransactionCommand_ShouldCreateExpenseTransaction_WithVendor()
    {
        // Arrange - Create vendor and contact person
        var contactPerson = new Person
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@feedsupply.com",
            PhoneNumber = "555-0200"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var vendor = new Vendor
        {
            Name = "Feed Supply Co.",
            Location = "Industrial Zone",
            ContactPersonId = contactPerson.Id
        };
        dbContext.Vendors.Add(vendor);
        await dbContext.SaveChangesAsync();

        var newTransaction = new NewTransactionDto(
            Title: "Feed purchase",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Expense",
            UnitPrice: 45.00m,
            Quantity: 20,
            TransactionAmount: 900.00m,
            Notes: "Monthly feed stock",
            ProductVariantId: null,
            BatchId: null,
            VendorId: vendor.Id,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("Expense", result.Value!.CreatedTransaction.Type);
        Assert.Equal(vendor.Id, result.Value!.CreatedTransaction.VendorId);
        Assert.Equal("Feed Supply Co.", result.Value!.CreatedTransaction.VendorName);
        Assert.Null(result.Value!.CreatedTransaction.CustomerId);
    }

    [Fact]
    public async Task CreateTransactionCommand_ShouldCreateTransaction_WithProductVariant()
    {
        // Arrange - Create product and variant
        var product = new Core.Models.Inventory.Product
        {
            Name = "Chicken Feed",
            Description = "Premium quality feed"
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var productVariant = new Core.Models.Inventory.ProductVariant
        {
            Name = "Starter Feed 25kg",
            ProductId = product.Id,
            Stock = 100,
            UnitOfMeasure = Core.Enums.UnitOfMeasure.Kilogram
        };
        dbContext.ProductVariants.Add(productVariant);
        await dbContext.SaveChangesAsync();

        var newTransaction = new NewTransactionDto(
            Title: "Feed purchase with variant",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Expense",
            UnitPrice: 30.00m,
            Quantity: 10,
            TransactionAmount: 300.00m,
            Notes: null,
            ProductVariantId: productVariant.Id,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(productVariant.Id, result.Value!.CreatedTransaction.ProductVariantId);
        Assert.Equal("Starter Feed 25kg", result.Value!.CreatedTransaction.ProductVariantName);
    }

    [Fact]
    public async Task CreateTransactionCommand_ShouldCreateTransaction_WithoutQuantity()
    {
        // Arrange - Transaction without quantity (service payment, for example)
        var newTransaction = new NewTransactionDto(
            Title: "Veterinary consultation",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Expense",
            UnitPrice: 150.00m,
            Quantity: null, // No quantity
            TransactionAmount: 150.00m,
            Notes: "Monthly health check",
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.CreatedTransaction.Quantity);
        Assert.Equal(150.00m, result.Value!.CreatedTransaction.TotalAmount); // Defaults to 1 * UnitPrice
        Assert.Equal(150.00m, result.Value!.CreatedTransaction.TransactionAmount);
    }

    [Fact]
    public async Task CreateTransactionCommand_ShouldCalculateTotalAmount_Correctly()
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "Equipment purchase",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Expense",
            UnitPrice: 125.50m,
            Quantity: 4,
            TransactionAmount: 500.00m, // Might differ from total due to discount
            Notes: "4 feeders with 10% discount",
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(502.00m, result.Value!.CreatedTransaction.TotalAmount); // 4 * 125.50
        Assert.Equal(500.00m, result.Value!.CreatedTransaction.TransactionAmount); // Actual amount paid
    }

    [Fact]
    public async Task CreateTransactionCommand_ShouldReturnValidationError_ForEmptyTitle()
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "", // Invalid: Title is required
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Income",
            UnitPrice: 100.00m,
            Quantity: 1,
            TransactionAmount: 100.00m,
            Notes: null,
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "title");
    }

    [Fact]
    public async Task CreateTransactionCommand_ShouldReturnValidationError_ForTitleExceedingMaxLength()
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: new string('A', 201), // Invalid: Title exceeds max length (200)
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Income",
            UnitPrice: 100.00m,
            Quantity: 1,
            TransactionAmount: 100.00m,
            Notes: null,
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "title");
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateTransactionCommand_ShouldReturnValidationError_ForInvalidDate()
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: "invalid-date", // Invalid date format
            Type: "Income",
            UnitPrice: 100.00m,
            Quantity: 1,
            TransactionAmount: 100.00m,
            Notes: null,
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "dateClientIsoString");
    }

    [Fact]
    public async Task CreateTransactionCommand_ShouldReturnValidationError_ForInvalidTransactionType()
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "InvalidType", // Invalid transaction type
            UnitPrice: 100.00m,
            Quantity: 1,
            TransactionAmount: 100.00m,
            Notes: null,
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "type");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-0.01)]
    public async Task CreateTransactionCommand_ShouldReturnValidationError_ForInvalidUnitPrice(decimal unitPrice)
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Income",
            UnitPrice: unitPrice, // Invalid: UnitPrice must be greater than zero
            Quantity: 1,
            TransactionAmount: 100.00m,
            Notes: null,
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "unitPrice");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(-1)]
    public async Task CreateTransactionCommand_ShouldReturnValidationError_ForInvalidQuantity(int quantity)
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Income",
            UnitPrice: 100.00m,
            Quantity: quantity, // Invalid: Quantity must be greater than zero if provided
            TransactionAmount: 100.00m,
            Notes: null,
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "quantity");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public async Task CreateTransactionCommand_ShouldReturnValidationError_ForInvalidTransactionAmount(decimal amount)
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Income",
            UnitPrice: 100.00m,
            Quantity: 1,
            TransactionAmount: amount, // Invalid: TransactionAmount must be greater than zero
            Notes: null,
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "transactionAmount");
    }

    [Fact]
    public async Task CreateTransactionCommand_ShouldReturnValidationError_ForNotesExceedingMaxLength()
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Income",
            UnitPrice: 100.00m,
            Quantity: 1,
            TransactionAmount: 100.00m,
            Notes: new string('N', 1001), // Invalid: Notes exceed max length (1000)
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "notes");
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateTransactionCommand_ShouldReturnValidationError_ForNonExistentProductVariant()
    {
        // Arrange
        var nonExistentProductVariantId = Guid.NewGuid();
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Expense",
            UnitPrice: 100.00m,
            Quantity: 1,
            TransactionAmount: 100.00m,
            Notes: null,
            ProductVariantId: nonExistentProductVariantId, // Non-existent product variant
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "productVariantId");
    }

    [Fact]
    public async Task CreateTransactionCommand_ShouldReturnValidationError_ForNonExistentBatch()
    {
        // Arrange
        var nonExistentBatchId = Guid.NewGuid();
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Income",
            UnitPrice: 100.00m,
            Quantity: 1,
            TransactionAmount: 100.00m,
            Notes: null,
            ProductVariantId: null,
            BatchId: nonExistentBatchId, // Non-existent batch
            VendorId: null,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchId");
    }

    [Fact]
    public async Task CreateTransactionCommand_ShouldReturnValidationError_ForNonExistentVendor()
    {
        // Arrange
        var nonExistentVendorId = Guid.NewGuid();
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Expense",
            UnitPrice: 100.00m,
            Quantity: 1,
            TransactionAmount: 100.00m,
            Notes: null,
            ProductVariantId: null,
            BatchId: null,
            VendorId: nonExistentVendorId, // Non-existent vendor
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "vendorId");
    }

    [Fact]
    public async Task CreateTransactionCommand_ShouldReturnValidationError_ForNonExistentCustomer()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Income",
            UnitPrice: 100.00m,
            Quantity: 1,
            TransactionAmount: 100.00m,
            Notes: null,
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: nonExistentCustomerId // Non-existent customer
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "customerId");
    }

    [Fact]
    public async Task CreateTransactionCommand_ShouldReturnValidationErrors_ForMultipleInvalidFields()
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "", // Invalid: empty
            DateClientIsoString: "bad-date", // Invalid: bad format
            Type: "BadType", // Invalid: not a valid type
            UnitPrice: 0, // Invalid: must be > 0
            Quantity: -5, // Invalid: must be > 0 if provided
            TransactionAmount: -100, // Invalid: must be > 0
            Notes: new string('N', 1001), // Invalid: exceeds max length
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var request = new AppRequest<CreateTransactionCommand.Args>(new(newTransaction));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "title");
        Assert.Contains(result.ValidationErrors, e => e.field == "dateClientIsoString");
        Assert.Contains(result.ValidationErrors, e => e.field == "type");
        Assert.Contains(result.ValidationErrors, e => e.field == "unitPrice");
        Assert.Contains(result.ValidationErrors, e => e.field == "quantity");
        Assert.Contains(result.ValidationErrors, e => e.field == "transactionAmount");
        Assert.Contains(result.ValidationErrors, e => e.field == "notes");
        Assert.True(result.ValidationErrors.Count() >= 7);
    }
}

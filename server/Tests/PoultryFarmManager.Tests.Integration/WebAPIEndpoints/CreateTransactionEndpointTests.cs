using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Core.Models.Finance;
using PoultryFarmManager.WebAPI.Endpoints.v1.Transactions;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class CreateTransactionEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task POST_Transaction_ValidIncomeTransaction_ShouldReturnCreatedWithLocationHeader()
    {
        // Arrange - Create customer
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
            Name = "Test Batch for Transaction API",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed API-1"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var newTransaction = new NewTransactionDto(
            Title: "Sale of chickens",
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
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreateTransactionEndpoint.CreateTransactionResponseBody>();

        // Assert - HTTP-specific concerns
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/api/v1/transactions/", response.Headers.Location.ToString());

        // Response body structure
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Transaction);
        Assert.NotEqual(Guid.Empty, responseBody.Transaction.Id);
        Assert.Equal("Sale of chickens", responseBody.Transaction.Title);
        Assert.Equal("Income", responseBody.Transaction.Type);
        Assert.Equal(15.50m, responseBody.Transaction.UnitPrice);
        Assert.Equal(50, responseBody.Transaction.Quantity);
        Assert.Equal(775.00m, responseBody.Transaction.TransactionAmount);
        Assert.Equal(775.00m, responseBody.Transaction.TotalAmount);
        Assert.Equal("Sold to local market", responseBody.Transaction.Notes);
        Assert.Equal(batch.Id, responseBody.Transaction.BatchId);
        Assert.Equal(customer.Id, responseBody.Transaction.CustomerId);
        Assert.Equal("John Doe", responseBody.Transaction.CustomerName);
    }

    [Fact]
    public async Task POST_Transaction_ValidExpenseTransaction_ShouldReturnCreated()
    {
        // Arrange - Create vendor
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
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreateTransactionEndpoint.CreateTransactionResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Equal("Expense", responseBody.Transaction.Type);
        Assert.Equal(vendor.Id, responseBody.Transaction.VendorId);
        Assert.Equal("Feed Supply Co.", responseBody.Transaction.VendorName);
    }

    [Fact]
    public async Task POST_Transaction_WithProductVariant_ShouldReturnCreated()
    {
        // Arrange - Create product variant
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
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreateTransactionEndpoint.CreateTransactionResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(productVariant.Id, responseBody!.Transaction.ProductVariantId);
        Assert.Equal("Starter Feed 25kg", responseBody.Transaction.ProductVariantName);
    }

    [Fact]
    public async Task POST_Transaction_WithoutQuantity_ShouldReturnCreated()
    {
        // Arrange - Service payment without quantity
        var newTransaction = new NewTransactionDto(
            Title: "Veterinary consultation",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Expense",
            UnitPrice: 150.00m,
            Quantity: null,
            TransactionAmount: 150.00m,
            Notes: "Monthly health check",
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreateTransactionEndpoint.CreateTransactionResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Null(responseBody!.Transaction.Quantity);
        Assert.Equal(150.00m, responseBody.Transaction.TotalAmount);
    }

    [Fact]
    public async Task POST_Transaction_EmptyTitle_ShouldReturn400()
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "", // Invalid
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
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_Transaction_TitleExceedingMaxLength_ShouldReturn400()
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: new string('A', 201), // Exceeds 200 character limit
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
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_Transaction_InvalidDate_ShouldReturn400()
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: "invalid-date",
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
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_Transaction_InvalidTransactionType_ShouldReturn400()
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "InvalidType",
            UnitPrice: 100.00m,
            Quantity: 1,
            TransactionAmount: 100.00m,
            Notes: null,
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task POST_Transaction_InvalidUnitPrice_ShouldReturn400(decimal unitPrice)
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Income",
            UnitPrice: unitPrice,
            Quantity: 1,
            TransactionAmount: 100.00m,
            Notes: null,
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task POST_Transaction_InvalidQuantity_ShouldReturn400(int quantity)
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Income",
            UnitPrice: 100.00m,
            Quantity: quantity,
            TransactionAmount: 100.00m,
            Notes: null,
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task POST_Transaction_InvalidTransactionAmount_ShouldReturn400(decimal amount)
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Income",
            UnitPrice: 100.00m,
            Quantity: 1,
            TransactionAmount: amount,
            Notes: null,
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_Transaction_NotesExceedingMaxLength_ShouldReturn400()
    {
        // Arrange
        var newTransaction = new NewTransactionDto(
            Title: "Test Transaction",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Type: "Income",
            UnitPrice: 100.00m,
            Quantity: 1,
            TransactionAmount: 100.00m,
            Notes: new string('N', 1001), // Exceeds 1000 character limit
            ProductVariantId: null,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_Transaction_NonExistentProductVariant_ShouldReturn400()
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
            ProductVariantId: nonExistentProductVariantId,
            BatchId: null,
            VendorId: null,
            CustomerId: null
        );
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_Transaction_NonExistentBatch_ShouldReturn400()
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
            BatchId: nonExistentBatchId,
            VendorId: null,
            CustomerId: null
        );
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_Transaction_NonExistentVendor_ShouldReturn400()
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
            VendorId: nonExistentVendorId,
            CustomerId: null
        );
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_Transaction_NonExistentCustomer_ShouldReturn400()
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
            CustomerId: nonExistentCustomerId
        );
        var body = new { transaction = newTransaction };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/transactions", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

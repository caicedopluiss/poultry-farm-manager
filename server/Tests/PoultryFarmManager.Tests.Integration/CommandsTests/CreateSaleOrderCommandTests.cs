using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Commands.SaleOrders;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class CreateSaleOrderCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<CreateSaleOrderCommand.Args, CreateSaleOrderCommand.Result> handler =
        fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<CreateSaleOrderCommand.Args, CreateSaleOrderCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    private static NewSaleOrderItemDto Item(decimal weight = 2.5m, string uom = "Kilogram") =>
        new(weight, uom, DateTime.UtcNow.ToString(Constants.DateTimeFormat));

    private async Task<(Batch batch, Person customer)> SeedDependenciesAsync()
    {
        var customer = new Person { FirstName = "John", LastName = "Doe" };
        dbContext.Persons.Add(customer);

        var batch = new Batch
        {
            Name = "Test Batch",
            StartDate = DateTime.UtcNow,
            MaleCount = 50,
            FemaleCount = 50,
            UnsexedCount = 0,
            InitialPopulation = 100,
            Status = BatchStatus.Active,
            Shed = "Shed A-1"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        return (batch, customer);
    }

    [Fact]
    public async Task CreateSaleOrderCommand_ShouldCreateSaleOrder()
    {
        // Arrange
        var (batch, customer) = await SeedDependenciesAsync();

        var dto = new NewSaleOrderDto(
            BatchId: batch.Id,
            CustomerId: customer.Id,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            PricePerUnit: 12.50m,
            Items: [Item(2.5m), Item(3.0m)],
            Notes: "Test order");
        var request = new AppRequest<CreateSaleOrderCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var order = result.Value!.CreatedSaleOrder;
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(batch.Id, order.BatchId);
        Assert.Equal(customer.Id, order.CustomerId);
        Assert.Equal(nameof(SaleOrderStatus.Pending), order.Status);
        Assert.Equal(12.50m, order.PricePerUnit);
        Assert.Equal(2, order.Items.Count());
        Assert.Equal(5.5m, order.TotalWeight);      // 2.5 + 3.0
        Assert.Equal(68.75m, order.TotalAmount);    // 5.5 * 12.50
        Assert.Equal(0m, order.TotalPaid);
        Assert.Equal(68.75m, order.PendingAmount);
        Assert.Equal("Test order", order.Notes);
        Assert.Empty(order.Payments);
    }

    [Fact]
    public async Task CreateSaleOrderCommand_ShouldReturnValidationError_WhenBatchIdIsEmpty()
    {
        // Arrange
        var customer = new Person { FirstName = "Jane", LastName = "Doe" };
        dbContext.Persons.Add(customer);
        await dbContext.SaveChangesAsync();

        var dto = new NewSaleOrderDto(Guid.Empty, customer.Id,
            DateTime.UtcNow.ToString(Constants.DateTimeFormat), 10m, [Item()], null);
        var request = new AppRequest<CreateSaleOrderCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchId");
    }

    [Fact]
    public async Task CreateSaleOrderCommand_ShouldReturnValidationError_WhenBatchNotFound()
    {
        // Arrange
        var customer = new Person { FirstName = "Jane", LastName = "Doe" };
        dbContext.Persons.Add(customer);
        await dbContext.SaveChangesAsync();

        var dto = new NewSaleOrderDto(Guid.NewGuid(), customer.Id,
            DateTime.UtcNow.ToString(Constants.DateTimeFormat), 10m, [Item()], null);
        var request = new AppRequest<CreateSaleOrderCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchId" && e.error.Contains("not found"));
    }

    [Fact]
    public async Task CreateSaleOrderCommand_ShouldReturnValidationError_WhenCustomerNotFound()
    {
        // Arrange
        var (batch, _) = await SeedDependenciesAsync();

        var dto = new NewSaleOrderDto(batch.Id, Guid.NewGuid(),
            DateTime.UtcNow.ToString(Constants.DateTimeFormat), 10m, [Item()], null);
        var request = new AppRequest<CreateSaleOrderCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "customerId" && e.error.Contains("not found"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task CreateSaleOrderCommand_ShouldReturnValidationError_WhenPricePerUnitIsInvalid(decimal price)
    {
        // Arrange
        var (batch, customer) = await SeedDependenciesAsync();

        var dto = new NewSaleOrderDto(batch.Id, customer.Id,
            DateTime.UtcNow.ToString(Constants.DateTimeFormat), price, [Item()], null);
        var request = new AppRequest<CreateSaleOrderCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "pricePerUnit");
    }

    [Fact]
    public async Task CreateSaleOrderCommand_ShouldReturnValidationError_WhenItemsIsEmpty()
    {
        // Arrange
        var (batch, customer) = await SeedDependenciesAsync();

        var dto = new NewSaleOrderDto(batch.Id, customer.Id,
            DateTime.UtcNow.ToString(Constants.DateTimeFormat), 10m, [], null);
        var request = new AppRequest<CreateSaleOrderCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "items");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateSaleOrderCommand_ShouldReturnValidationError_WhenItemWeightIsInvalid(decimal weight)
    {
        // Arrange
        var (batch, customer) = await SeedDependenciesAsync();

        var dto = new NewSaleOrderDto(batch.Id, customer.Id,
            DateTime.UtcNow.ToString(Constants.DateTimeFormat), 10m, [Item(weight)], null);
        var request = new AppRequest<CreateSaleOrderCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field.Contains("weight"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("InvalidUnit")]
    [InlineData("Liter")]
    public async Task CreateSaleOrderCommand_ShouldReturnValidationError_WhenItemUnitOfMeasureIsInvalid(string uom)
    {
        // Arrange
        var (batch, customer) = await SeedDependenciesAsync();

        var dto = new NewSaleOrderDto(batch.Id, customer.Id,
            DateTime.UtcNow.ToString(Constants.DateTimeFormat), 10m, [Item(2m, uom)], null);
        var request = new AppRequest<CreateSaleOrderCommand.Args>(new(dto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field.Contains("unitOfMeasure"));
    }
}

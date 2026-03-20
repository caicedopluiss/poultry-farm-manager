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

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class AddSaleOrderPaymentCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<AddSaleOrderPaymentCommand.Args, AddSaleOrderPaymentCommand.Result> handler =
        fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<AddSaleOrderPaymentCommand.Args, AddSaleOrderPaymentCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    private static AddSaleOrderPaymentDto Payment(decimal amount, string? notes = null) =>
        new(DateTime.UtcNow.ToString(Constants.DateTimeFormat), amount, notes);

    [Fact]
    public async Task AddSaleOrderPaymentCommand_ShouldAddPayment_AndSetStatusToPartiallyPaid()
    {
        // Arrange - sale order: 2.5kg @ $10/kg = $25 total
        var saleOrder = await dbContext.CreateSaleOrderAsync(pricePerKg: 10m);
        var request = new AppRequest<AddSaleOrderPaymentCommand.Args>(new(saleOrder.Id, Payment(10m, "First payment")));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        var order = result.Value!.UpdatedSaleOrder;
        Assert.Equal(nameof(SaleOrderStatus.PartiallyPaid), order.Status);
        Assert.Equal(10m, order.TotalPaid);
        Assert.Equal(15m, order.PendingAmount);   // 25 - 10
        Assert.Single(order.Payments);
        Assert.Equal("First payment", order.Payments.First().Notes);
    }

    [Fact]
    public async Task AddSaleOrderPaymentCommand_ShouldSetStatusToPaid_WhenFullAmountIsPaidInOnePayment()
    {
        // Arrange - sale order: 2.5kg @ $10/kg = $25 total
        var saleOrder = await dbContext.CreateSaleOrderAsync(pricePerKg: 10m);
        var request = new AppRequest<AddSaleOrderPaymentCommand.Args>(new(saleOrder.Id, Payment(25m)));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var order = result.Value!.UpdatedSaleOrder;
        Assert.Equal(nameof(SaleOrderStatus.Paid), order.Status);
        Assert.Equal(25m, order.TotalPaid);
        Assert.Equal(0m, order.PendingAmount);
    }

    [Fact]
    public async Task AddSaleOrderPaymentCommand_ShouldSetStatusToPaid_AfterMultiplePayments()
    {
        // Arrange - sale order: 2.5kg @ $10/kg = $25 total
        var saleOrder = await dbContext.CreateSaleOrderAsync(pricePerKg: 10m);

        // First payment: $10 → PartiallyPaid
        await handler.HandleAsync(
            new AppRequest<AddSaleOrderPaymentCommand.Args>(new(saleOrder.Id, Payment(10m))),
            CancellationToken.None);

        // Second payment: $15 → Paid
        var finalRequest = new AppRequest<AddSaleOrderPaymentCommand.Args>(new(saleOrder.Id, Payment(15m)));

        // Act
        var result = await handler.HandleAsync(finalRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var order = result.Value!.UpdatedSaleOrder;
        Assert.Equal(nameof(SaleOrderStatus.Paid), order.Status);
        Assert.Equal(25m, order.TotalPaid);
        Assert.Equal(2, order.Payments.Count());
    }

    [Fact]
    public async Task AddSaleOrderPaymentCommand_ShouldReturnValidationError_WhenSaleOrderNotFound()
    {
        // Arrange
        var request = new AppRequest<AddSaleOrderPaymentCommand.Args>(new(Guid.NewGuid(), Payment(10m)));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "saleOrderId" && e.error.Contains("not found"));
    }

    [Fact]
    public async Task AddSaleOrderPaymentCommand_ShouldReturnValidationError_WhenSaleOrderIsCancelled()
    {
        // Arrange
        var saleOrder = await dbContext.CreateSaleOrderAsync(status: SaleOrderStatus.Cancelled);
        var request = new AppRequest<AddSaleOrderPaymentCommand.Args>(new(saleOrder.Id, Payment(10m)));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "saleOrderId" && e.error.Contains("cancelled"));
    }

    [Fact]
    public async Task AddSaleOrderPaymentCommand_ShouldReturnValidationError_WhenSaleOrderIsAlreadyPaid()
    {
        // Arrange
        var saleOrder = await dbContext.CreateSaleOrderAsync(status: SaleOrderStatus.Paid);
        var request = new AppRequest<AddSaleOrderPaymentCommand.Args>(new(saleOrder.Id, Payment(10m)));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "saleOrderId" && e.error.Contains("paid"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task AddSaleOrderPaymentCommand_ShouldReturnValidationError_WhenAmountIsInvalid(decimal amount)
    {
        // Arrange
        var saleOrder = await dbContext.CreateSaleOrderAsync();
        var request = new AppRequest<AddSaleOrderPaymentCommand.Args>(new(saleOrder.Id, Payment(amount)));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "amount");
    }
}

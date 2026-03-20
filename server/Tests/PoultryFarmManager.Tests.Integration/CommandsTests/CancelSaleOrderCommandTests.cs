using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.SaleOrders;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class CancelSaleOrderCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<CancelSaleOrderCommand.Args, CancelSaleOrderCommand.Result> handler =
        fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<CancelSaleOrderCommand.Args, CancelSaleOrderCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CancelSaleOrderCommand_ShouldCancelSaleOrder()
    {
        // Arrange
        var saleOrder = await dbContext.CreateSaleOrderAsync();
        var request = new AppRequest<CancelSaleOrderCommand.Args>(new(saleOrder.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(nameof(SaleOrderStatus.Cancelled), result.Value!.CancelledSaleOrder.Status);
        Assert.Equal(saleOrder.Id, result.Value.CancelledSaleOrder.Id);
    }

    [Fact]
    public async Task CancelSaleOrderCommand_ShouldReturnValidationError_WhenSaleOrderIdIsEmpty()
    {
        // Arrange
        var request = new AppRequest<CancelSaleOrderCommand.Args>(new(Guid.Empty));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "saleOrderId");
    }

    [Fact]
    public async Task CancelSaleOrderCommand_ShouldReturnValidationError_WhenSaleOrderNotFound()
    {
        // Arrange
        var request = new AppRequest<CancelSaleOrderCommand.Args>(new(Guid.NewGuid()));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "saleOrderId" && e.error.Contains("not found"));
    }

    [Fact]
    public async Task CancelSaleOrderCommand_ShouldReturnValidationError_WhenSaleOrderIsAlreadyCancelled()
    {
        // Arrange
        var saleOrder = await dbContext.CreateSaleOrderAsync(status: SaleOrderStatus.Cancelled);
        var request = new AppRequest<CancelSaleOrderCommand.Args>(new(saleOrder.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "saleOrderId" && e.error.Contains("already cancelled"));
    }

    [Fact]
    public async Task CancelSaleOrderCommand_ShouldReturnValidationError_WhenSaleOrderIsPaid()
    {
        // Arrange
        var saleOrder = await dbContext.CreateSaleOrderAsync(status: SaleOrderStatus.Paid);
        var request = new AppRequest<CancelSaleOrderCommand.Args>(new(saleOrder.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "saleOrderId" && e.error.Contains("paid"));
    }
}

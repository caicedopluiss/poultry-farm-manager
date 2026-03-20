using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.SaleOrders;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetSaleOrderByIdQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetSaleOrderByIdQuery.Args, GetSaleOrderByIdQuery.Result> handler =
        fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetSaleOrderByIdQuery.Args, GetSaleOrderByIdQuery.Result>>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GetSaleOrderByIdQuery_ShouldReturnNull_WhenSaleOrderDoesNotExist()
    {
        // Arrange
        var request = new AppRequest<GetSaleOrderByIdQuery.Args>(new(Guid.NewGuid()));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value.SaleOrder);
    }

    [Fact]
    public async Task GetSaleOrderByIdQuery_ShouldReturnSaleOrder_WhenSaleOrderExists()
    {
        // Arrange
        var saleOrder = await dbContext.CreateSaleOrderAsync(pricePerKg: 15m);
        var request = new AppRequest<GetSaleOrderByIdQuery.Args>(new(saleOrder.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value.SaleOrder);
        var dto = result.Value.SaleOrder!;
        Assert.Equal(saleOrder.Id, dto.Id);
        Assert.Equal(saleOrder.BatchId, dto.BatchId);
        Assert.Equal(saleOrder.CustomerId, dto.CustomerId);
        Assert.Equal(15m, dto.PricePerUnit);
        Assert.Single(dto.Items);
        Assert.Empty(dto.Payments);
        Assert.Equal(2.5m, dto.TotalWeight);
        Assert.Equal(37.5m, dto.TotalAmount);   // 2.5 * 15
        Assert.Equal(0m, dto.TotalPaid);
        Assert.Equal(37.5m, dto.PendingAmount);
    }

    [Fact]
    public async Task GetSaleOrderByIdQuery_ShouldReturnValidationError_WhenIdIsEmpty()
    {
        // Arrange
        var request = new AppRequest<GetSaleOrderByIdQuery.Args>(new(Guid.Empty));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "id");
    }
}

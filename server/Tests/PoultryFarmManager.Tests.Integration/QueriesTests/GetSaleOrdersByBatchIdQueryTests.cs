using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.SaleOrders;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetSaleOrdersByBatchIdQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetSaleOrdersByBatchIdQuery.Args, GetSaleOrdersByBatchIdQuery.Result> handler =
        fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetSaleOrdersByBatchIdQuery.Args, GetSaleOrdersByBatchIdQuery.Result>>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GetSaleOrdersByBatchIdQuery_ShouldReturnEmptyList_WhenBatchHasNoSaleOrders()
    {
        // Arrange
        var request = new AppRequest<GetSaleOrdersByBatchIdQuery.Args>(new(Guid.NewGuid()));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.SaleOrders);
    }

    [Fact]
    public async Task GetSaleOrdersByBatchIdQuery_ShouldReturnSaleOrders_ForBatch()
    {
        // Arrange - two sale orders for the same batch
        var order1 = await dbContext.CreateSaleOrderAsync(pricePerKg: 10m);
        var batchId = order1.BatchId;
        var customerId = order1.CustomerId;

        // Second order reusing the same batch and customer
        var order2 = new Core.Models.SaleOrder
        {
            BatchId = batchId,
            CustomerId = customerId,
            Date = DateTime.UtcNow,
            Status = Core.Enums.SaleOrderStatus.Pending,
            PricePerKg = 12m,
            Items =
            [
                new Core.Models.SaleOrderItem
                {
                    Weight = 3m,
                    UnitOfMeasure = Core.Enums.UnitOfMeasure.Kilogram,
                    ProcessedDate = DateTime.UtcNow
                }
            ]
        };
        dbContext.SaleOrders.Add(order2);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetSaleOrdersByBatchIdQuery.Args>(new(batchId));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.SaleOrders.Count());
        Assert.All(result.Value.SaleOrders, o => Assert.Equal(batchId, o.BatchId));
    }

    [Fact]
    public async Task GetSaleOrdersByBatchIdQuery_ShouldReturnValidationError_WhenBatchIdIsEmpty()
    {
        // Arrange
        var request = new AppRequest<GetSaleOrdersByBatchIdQuery.Args>(new(Guid.Empty));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchId");
    }
}

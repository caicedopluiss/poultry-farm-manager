using System;
using System.Net;
using System.Net.Http.Json;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.WebAPI.Endpoints.v1.SaleOrders;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetSaleOrdersByBatchIdEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_SaleOrdersByBatchId_ShouldReturnOkWithEmptyList_WhenNoSaleOrdersExist()
    {
        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/batches/{Guid.NewGuid()}/sale-orders");
        var responseBody = await response.Content.ReadFromJsonAsync<GetSaleOrdersByBatchIdEndpoint.GetSaleOrdersByBatchIdResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Empty(responseBody!.SaleOrders);
    }

    [Fact]
    public async Task GET_SaleOrdersByBatchId_ShouldReturnSaleOrders_WhenBatchHasSaleOrders()
    {
        // Arrange
        var saleOrder = await dbContext.CreateSaleOrderAsync();

        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/batches/{saleOrder.BatchId}/sale-orders");
        var responseBody = await response.Content.ReadFromJsonAsync<GetSaleOrdersByBatchIdEndpoint.GetSaleOrdersByBatchIdResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Single(responseBody!.SaleOrders);
        Assert.Equal(saleOrder.Id, responseBody.SaleOrders.First().Id);
    }

    [Fact]
    public async Task GET_SaleOrdersByBatchId_ShouldReturnBadRequest_WhenBatchIdIsEmpty()
    {
        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/batches/{Guid.Empty}/sale-orders");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

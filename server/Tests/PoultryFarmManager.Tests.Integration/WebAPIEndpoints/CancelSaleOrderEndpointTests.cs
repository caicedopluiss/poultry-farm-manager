using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.WebAPI.Endpoints.v1.SaleOrders;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class CancelSaleOrderEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task POST_CancelSaleOrder_ValidRequest_ShouldReturnOkAndCancelledStatus()
    {
        // Arrange
        var saleOrder = await dbContext.CreateSaleOrderAsync();

        // Act
        var response = await fixture.Client.PostAsync($"/api/v1/sale-orders/{saleOrder.Id}/cancel", null);
        var responseBody = await response.Content.ReadFromJsonAsync<CancelSaleOrderEndpoint.CancelSaleOrderResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody!.SaleOrder);
        Assert.Equal(nameof(SaleOrderStatus.Cancelled), responseBody.SaleOrder.Status);
        Assert.Equal(saleOrder.Id, responseBody.SaleOrder.Id);
    }

    [Fact]
    public async Task POST_CancelSaleOrder_SaleOrderNotFound_ShouldReturnNotFound()
    {
        // Act
        var response = await fixture.Client.PostAsync($"/api/v1/sale-orders/{Guid.NewGuid()}/cancel", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task POST_CancelSaleOrder_AlreadyCancelled_ShouldReturnBadRequest()
    {
        // Arrange
        var saleOrder = await dbContext.CreateSaleOrderAsync(status: SaleOrderStatus.Cancelled);

        // Act
        var response = await fixture.Client.PostAsync($"/api/v1/sale-orders/{saleOrder.Id}/cancel", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_CancelSaleOrder_AlreadyPaid_ShouldReturnBadRequest()
    {
        // Arrange
        var saleOrder = await dbContext.CreateSaleOrderAsync(status: SaleOrderStatus.Paid);

        // Act
        var response = await fixture.Client.PostAsync($"/api/v1/sale-orders/{saleOrder.Id}/cancel", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

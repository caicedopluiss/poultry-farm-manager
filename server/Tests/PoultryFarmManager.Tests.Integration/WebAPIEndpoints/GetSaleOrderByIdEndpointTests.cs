using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.WebAPI.Endpoints.v1.SaleOrders;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetSaleOrderByIdEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_SaleOrderById_ShouldReturnNotFound_WhenSaleOrderDoesNotExist()
    {
        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/sale-orders/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GET_SaleOrderById_ShouldReturnOkAndSaleOrder_WhenSaleOrderExists()
    {
        // Arrange
        var saleOrder = await dbContext.CreateSaleOrderAsync(pricePerKg: 20m);

        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/sale-orders/{saleOrder.Id}");
        var responseBody = await response.Content.ReadFromJsonAsync<GetSaleOrderByIdEndpoint.GetSaleOrderByIdResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody!.SaleOrder);
        Assert.Equal(saleOrder.Id, responseBody.SaleOrder.Id);
        Assert.Equal(20m, responseBody.SaleOrder.PricePerUnit);
    }

    [Fact]
    public async Task GET_SaleOrderById_ShouldReturnBadRequest_WhenIdIsEmpty()
    {
        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/sale-orders/{Guid.Empty}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

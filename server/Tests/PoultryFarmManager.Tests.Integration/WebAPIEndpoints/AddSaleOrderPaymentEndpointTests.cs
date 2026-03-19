using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.WebAPI.Endpoints.v1.SaleOrders;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class AddSaleOrderPaymentEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task POST_SaleOrderPayments_ValidRequest_ShouldReturnOkAndUpdateStatus()
    {
        // Arrange - sale order: 2.5kg @ $10/kg = $25 total
        var saleOrder = await dbContext.CreateSaleOrderAsync(pricePerKg: 10m);
        var payment = new AddSaleOrderPaymentDto(
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Amount: 10m,
            Notes: "Partial payment");
        var body = new { payment };

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/sale-orders/{saleOrder.Id}/payments", body);
        var responseBody = await response.Content.ReadFromJsonAsync<AddSaleOrderPaymentEndpoint.AddPaymentResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody!.SaleOrder);
        Assert.Equal(nameof(SaleOrderStatus.PartiallyPaid), responseBody.SaleOrder.Status);
        Assert.Equal(10m, responseBody.SaleOrder.TotalPaid);
        Assert.Equal(15m, responseBody.SaleOrder.PendingAmount);
    }

    [Fact]
    public async Task POST_SaleOrderPayments_FullPayment_ShouldSetStatusToPaid()
    {
        // Arrange - sale order: 2.5kg @ $10/kg = $25 total
        var saleOrder = await dbContext.CreateSaleOrderAsync(pricePerKg: 10m);
        var payment = new AddSaleOrderPaymentDto(
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Amount: 25m,
            Notes: null);
        var body = new { payment };

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/sale-orders/{saleOrder.Id}/payments", body);
        var responseBody = await response.Content.ReadFromJsonAsync<AddSaleOrderPaymentEndpoint.AddPaymentResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Equal(nameof(SaleOrderStatus.Paid), responseBody!.SaleOrder.Status);
        Assert.Equal(0m, responseBody.SaleOrder.PendingAmount);
    }

    [Fact]
    public async Task POST_SaleOrderPayments_InvalidRequest_ZeroAmount_ShouldReturnBadRequest()
    {
        // Arrange
        var saleOrder = await dbContext.CreateSaleOrderAsync();
        var payment = new AddSaleOrderPaymentDto(
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Amount: 0m,
            Notes: null);
        var body = new { payment };

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/sale-orders/{saleOrder.Id}/payments", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_SaleOrderPayments_SaleOrderNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var payment = new AddSaleOrderPaymentDto(
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Amount: 10m,
            Notes: null);
        var body = new { payment };

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/sale-orders/{Guid.NewGuid()}/payments", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

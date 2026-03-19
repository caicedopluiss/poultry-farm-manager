using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.Core.Models.Finance;
using PoultryFarmManager.WebAPI.Endpoints.v1.SaleOrders;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class CreateSaleOrderEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    private async Task<(Batch batch, Person customer)> SeedDependenciesAsync()
    {
        var customer = new Person { FirstName = "API", LastName = "Test" };
        dbContext.Persons.Add(customer);

        var batch = new Batch
        {
            Name = "API Test Batch",
            StartDate = DateTime.UtcNow,
            MaleCount = 80,
            FemaleCount = 80,
            UnsexedCount = 0,
            InitialPopulation = 160,
            Status = BatchStatus.Active,
            Shed = "Shed API-1"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        return (batch, customer);
    }

    [Fact]
    public async Task POST_SaleOrders_ValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var (batch, customer) = await SeedDependenciesAsync();

        var newSaleOrder = new NewSaleOrderDto(
            BatchId: batch.Id,
            CustomerId: customer.Id,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            PricePerUnit: 15m,
            Items:
            [
                new NewSaleOrderItemDto(2.5m, "Kilogram", DateTime.UtcNow.ToString(Constants.DateTimeFormat)),
                new NewSaleOrderItemDto(3.0m, "Kilogram", DateTime.UtcNow.ToString(Constants.DateTimeFormat))
            ],
            Notes: "API test order");
        var body = new { newSaleOrder };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/sale-orders", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreateSaleOrderEndpoint.CreateSaleOrderResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/api/v1/sale-orders/", response.Headers.Location.ToString());
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody!.SaleOrder);
        Assert.NotEqual(Guid.Empty, responseBody.SaleOrder.Id);
        Assert.Equal(batch.Id, responseBody.SaleOrder.BatchId);
        Assert.Equal(customer.Id, responseBody.SaleOrder.CustomerId);
        Assert.Equal(nameof(SaleOrderStatus.Pending), responseBody.SaleOrder.Status);
        Assert.Equal(15m, responseBody.SaleOrder.PricePerUnit);
        Assert.Equal(2, responseBody.SaleOrder.Items.Count());
        Assert.Equal(5.5m, responseBody.SaleOrder.TotalWeight);
        Assert.Equal(82.5m, responseBody.SaleOrder.TotalAmount);  // 5.5 * 15
        Assert.Equal("API test order", responseBody.SaleOrder.Notes);
    }

    [Fact]
    public async Task POST_SaleOrders_InvalidRequest_EmptyBatchId_ShouldReturnBadRequest()
    {
        // Arrange
        var customer = new Person { FirstName = "API", LastName = "Test" };
        dbContext.Persons.Add(customer);
        await dbContext.SaveChangesAsync();

        var newSaleOrder = new NewSaleOrderDto(
            BatchId: Guid.Empty,
            CustomerId: customer.Id,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            PricePerUnit: 10m,
            Items: [new NewSaleOrderItemDto(2m, "Kilogram", DateTime.UtcNow.ToString(Constants.DateTimeFormat))],
            Notes: null);
        var body = new { newSaleOrder };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/sale-orders", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_SaleOrders_InvalidRequest_EmptyItems_ShouldReturnBadRequest()
    {
        // Arrange
        var (batch, customer) = await SeedDependenciesAsync();

        var newSaleOrder = new NewSaleOrderDto(
            BatchId: batch.Id,
            CustomerId: customer.Id,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            PricePerUnit: 10m,
            Items: [],
            Notes: null);
        var body = new { newSaleOrder };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/sale-orders", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

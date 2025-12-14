using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;
using PoultryFarmManager.WebAPI.Endpoints.v1.Products;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetAllProductsEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_Products_ShouldReturnAllProducts()
    {
        // Arrange - Create test products
        var products = new[]
        {
            new Product
            {
                Name = "Premium Feed",
                Manufacturer = "FarmCo",
                UnitOfMeasure = UnitOfMeasure.Kilogram,
                Stock = 500m
            },
            new Product
            {
                Name = "Vitamins",
                Manufacturer = "BioVet",
                UnitOfMeasure = UnitOfMeasure.Liter,
                Stock = 100m
            }
        };
        dbContext.Products.AddRange(products);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await fixture.Client.GetAsync("/api/v1/products");
        var responseBody = await response.Content.ReadFromJsonAsync<GetAllProductsEndpoint.GetAllProductsResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Products);
        Assert.Equal(2, responseBody.Products.Count());
    }

    [Fact]
    public async Task GET_Products_ShouldReturnEmptyList_WhenNoProducts()
    {
        // Act
        var response = await fixture.Client.GetAsync("/api/v1/products");
        var responseBody = await response.Content.ReadFromJsonAsync<GetAllProductsEndpoint.GetAllProductsResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Products);
        Assert.Empty(responseBody.Products);
    }
}

using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;
using PoultryFarmManager.WebAPI.Endpoints.v1.Products;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetProductByIdEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_ProductById_ShouldReturnProduct()
    {
        // Arrange - Create test product
        var product = new Product
        {
            Name = "Premium Feed",
            Manufacturer = "FarmCo",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 500m,
            Description = "High quality feed"
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/products/{product.Id}");
        var responseBody = await response.Content.ReadFromJsonAsync<GetProductByIdEndpoint.GetProductByIdResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Product);
        Assert.Equal(product.Id, responseBody.Product.Id);
        Assert.Equal("Premium Feed", responseBody.Product.Name);
    }

    [Fact]
    public async Task GET_ProductById_NonExistentId_ShouldReturnNotFound()
    {
        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/products/{Guid.NewGuid()}");

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

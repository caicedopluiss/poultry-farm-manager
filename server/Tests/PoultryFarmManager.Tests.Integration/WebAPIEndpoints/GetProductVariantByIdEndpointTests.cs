using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;
using PoultryFarmManager.WebAPI.Endpoints.v1.ProductVariants;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetProductVariantByIdEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_ProductVariantById_ShouldReturnVariant()
    {
        // Arrange - Create a product and variant
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "FarmCo",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "25kg Bag",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m,
            Quantity = 25,
            Description = "25 kilogram bag"
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/product-variants/{variant.Id}");
        var responseBody = await response.Content.ReadFromJsonAsync<GetProductVariantByIdEndpoint.GetProductVariantByIdResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.ProductVariant);
        Assert.Equal(variant.Id, responseBody.ProductVariant.Id);
        Assert.Equal("25kg Bag", responseBody.ProductVariant.Name);
    }

    [Fact]
    public async Task GET_ProductVariantById_NonExistentId_ShouldReturnNotFound()
    {
        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/product-variants/{Guid.NewGuid()}");

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

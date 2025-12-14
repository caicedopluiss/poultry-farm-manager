using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;
using PoultryFarmManager.WebAPI.Endpoints.v1.ProductVariants;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetProductVariantsByProductIdEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_ProductVariantsByProductId_ShouldReturnVariants()
    {
        // Arrange - Create products and variants
        var product1 = new Product
        {
            Name = "Test Feed",
            Manufacturer = "FarmCo",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        var product2 = new Product
        {
            Name = "Vitamins",
            Manufacturer = "BioVet",
            UnitOfMeasure = UnitOfMeasure.Liter,
            Stock = 100m
        };
        dbContext.Products.AddRange(product1, product2);
        await dbContext.SaveChangesAsync();

        var variants = new[]
        {
            new ProductVariant
            {
                ProductId = product1.Id,
                Name = "25kg Bag",
                UnitOfMeasure = UnitOfMeasure.Kilogram,
                Stock = 50m,
                Quantity = 25
            },
            new ProductVariant
            {
                ProductId = product1.Id,
                Name = "50kg Bag",
                UnitOfMeasure = UnitOfMeasure.Kilogram,
                Stock = 30m,
                Quantity = 50
            },
            new ProductVariant
            {
                ProductId = product2.Id,
                Name = "1L Bottle",
                UnitOfMeasure = UnitOfMeasure.Liter,
                Stock = 100m,
                Quantity = 1
            }
        };
        dbContext.ProductVariants.AddRange(variants);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/products/{product1.Id}/variants");
        var responseBody = await response.Content.ReadFromJsonAsync<GetProductVariantsByProductIdEndpoint.GetProductVariantsByProductIdResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.ProductVariants);
        Assert.Equal(2, responseBody.ProductVariants.Count());
        Assert.All(responseBody.ProductVariants, v => Assert.Equal(product1.Id, v.ProductId));
    }

    [Fact]
    public async Task GET_ProductVariantsByProductId_ShouldReturnEmptyList_WhenNoVariants()
    {
        // Arrange - Create product without variants
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "FarmCo",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/products/{product.Id}/variants");
        var responseBody = await response.Content.ReadFromJsonAsync<GetProductVariantsByProductIdEndpoint.GetProductVariantsByProductIdResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.ProductVariants);
        Assert.Empty(responseBody.ProductVariants);
    }
}

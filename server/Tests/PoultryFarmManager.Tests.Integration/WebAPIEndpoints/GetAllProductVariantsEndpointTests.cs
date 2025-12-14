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

public class GetAllProductVariantsEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_ProductVariants_ShouldReturnAllVariants()
    {
        // Arrange - Create a product and variants
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "FarmCo",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var variants = new[]
        {
            new ProductVariant
            {
                ProductId = product.Id,
                Name = "25kg Bag",
                UnitOfMeasure = UnitOfMeasure.Kilogram,
                Stock = 100m,
                Quantity = 25
            },
            new ProductVariant
            {
                ProductId = product.Id,
                Name = "50kg Bag",
                UnitOfMeasure = UnitOfMeasure.Kilogram,
                Stock = 50m,
                Quantity = 50
            }
        };
        dbContext.ProductVariants.AddRange(variants);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await fixture.Client.GetAsync("/api/v1/product-variants");
        var responseBody = await response.Content.ReadFromJsonAsync<GetAllProductVariantsEndpoint.GetAllProductVariantsResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.ProductVariants);
        Assert.Equal(2, responseBody.ProductVariants.Count());
    }

    [Fact]
    public async Task GET_ProductVariants_ShouldReturnEmptyList_WhenNoVariants()
    {
        // Act
        var response = await fixture.Client.GetAsync("/api/v1/product-variants");
        var responseBody = await response.Content.ReadFromJsonAsync<GetAllProductVariantsEndpoint.GetAllProductVariantsResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.ProductVariants);
        Assert.Empty(responseBody.ProductVariants);
    }
}

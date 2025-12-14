using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;
using PoultryFarmManager.WebAPI.Endpoints.v1.ProductVariants;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class CreateProductVariantEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task POST_ProductVariants_ValidRequest_ShouldReturnCreated()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "FarmCo",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var newProductVariant = new
        {
            ProductId = product.Id,
            Name = "25kg Bag",
            UnitOfMeasure = nameof(UnitOfMeasure.Kilogram),
            Stock = 100m,
            Quantity = 25,
            Description = "25 kilogram bag"
        };
        var body = new { newProductVariant };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/product-variants", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreateProductVariantEndpoint.CreateProductVariantResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.ProductVariant);
        Assert.Equal("25kg Bag", responseBody.ProductVariant.Name);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"/api/v1/product-variants/{responseBody.ProductVariant.Id}", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task POST_ProductVariants_InvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "FarmCo",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var newProductVariant = new
        {
            ProductId = product.Id,
            Name = "", // Empty name should fail
            UnitOfMeasure = nameof(UnitOfMeasure.Kilogram),
            Stock = 100m,
            Quantity = 25,
            Description = "Test"
        };
        var body = new { newProductVariant };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/product-variants", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

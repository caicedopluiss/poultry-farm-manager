using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;
using PoultryFarmManager.WebAPI.Endpoints.v1.ProductVariants;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class UpdateProductVariantEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task PUT_ProductVariant_ValidRequest_ShouldReturnOk()
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
            Name = "Old Name",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m,
            Quantity = 10
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        var updateProductVariant = new
        {
            Name = "Updated Name",
            UnitOfMeasure = nameof(UnitOfMeasure.Gram),
            Stock = 250m,
            Quantity = 20,
            Description = "Updated description"
        };
        var body = new { updateProductVariant };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/product-variants/{variant.Id}", body);
        var responseBody = await response.Content.ReadFromJsonAsync<UpdateProductVariantEndpoint.UpdateProductVariantResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.ProductVariant);
        Assert.Equal(variant.Id, responseBody.ProductVariant.Id);
        Assert.Equal("Updated Name", responseBody.ProductVariant.Name);
        Assert.Equal(nameof(UnitOfMeasure.Gram), responseBody.ProductVariant.UnitOfMeasure);
    }

    [Fact]
    public async Task PUT_ProductVariant_NonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var updateProductVariant = new
        {
            Name = "Updated Name",
            UnitOfMeasure = nameof(UnitOfMeasure.Kilogram),
            Stock = 100m,
            Quantity = 10,
            Description = ""
        };
        var body = new { updateProductVariant };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/product-variants/{Guid.NewGuid()}", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

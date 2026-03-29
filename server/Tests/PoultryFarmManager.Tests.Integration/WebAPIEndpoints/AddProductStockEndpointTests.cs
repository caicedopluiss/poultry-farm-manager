using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;
using PoultryFarmManager.WebAPI.Endpoints.v1.Products;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class AddProductStockEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    // Helpers -----------------------------------------------------------------

    private async Task<(Product product, ProductVariant variant)> SeedProductWithVariantAsync(
        decimal productStock = 100m,
        decimal variantPackageSize = 25m,
        UnitOfMeasure productUnit = UnitOfMeasure.Kilogram,
        UnitOfMeasure? variantUnit = null)
    {
        var product = new Product
        {
            Name = $"Feed_{Guid.NewGuid().ToString()[..8]}",
            Manufacturer = "TestCo",
            UnitOfMeasure = productUnit,
            Stock = productStock
        };
        dbContext.Products.Add(product);

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Bag",
            UnitOfMeasure = variantUnit ?? productUnit,
            Stock = variantPackageSize
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        return (product, variant);
    }

    // Happy path --------------------------------------------------------------

    [Fact]
    public async Task POST_AddProductStock_ValidRequest_ShouldReturnOkAndUpdateProductStock()
    {
        // Arrange: 100 kg product + 25 kg/bag variant, add 4 bags → 200 kg
        var (product, variant) = await SeedProductWithVariantAsync(productStock: 100m, variantPackageSize: 25m);
        var body = new { productVariantId = variant.Id, quantity = 4 };

        // Act
        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/products/{product.Id}/add-stock", body);
        var responseBody = await response.Content.ReadFromJsonAsync<AddProductStockEndpoint.AddProductStockResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Equal(200m, responseBody!.UpdatedProduct.Stock);
    }

    [Fact]
    public async Task POST_AddProductStock_ValidRequest_ShouldNotChangeVariantStock()
    {
        // Arrange
        var (product, variant) = await SeedProductWithVariantAsync(variantPackageSize: 25m);
        var body = new { productVariantId = variant.Id, quantity = 3 };

        // Act
        await fixture.Client.PostAsJsonAsync($"/api/v1/products/{product.Id}/add-stock", body);

        // Assert: variant stock in DB is unchanged
        var variantInDb = await dbContext.ProductVariants.AsNoTracking().FirstAsync(v => v.Id == variant.Id);
        Assert.Equal(25m, variantInDb.Stock);
    }

    // Validation / error paths ------------------------------------------------

    [Fact]
    public async Task POST_AddProductStock_ProductNotFound_ShouldReturnBadRequest()
    {
        var (_, variant) = await SeedProductWithVariantAsync();
        var body = new { productVariantId = variant.Id, quantity = 1 };

        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/products/{Guid.NewGuid()}/add-stock", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_AddProductStock_VariantNotFound_ShouldReturnBadRequest()
    {
        var (product, _) = await SeedProductWithVariantAsync();
        var body = new { productVariantId = Guid.NewGuid(), quantity = 1 };

        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/products/{product.Id}/add-stock", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_AddProductStock_VariantNotBelongingToProduct_ShouldReturnBadRequest()
    {
        var (product1, _) = await SeedProductWithVariantAsync();
        var (_, variant2) = await SeedProductWithVariantAsync();
        var body = new { productVariantId = variant2.Id, quantity = 1 };

        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/products/{product1.Id}/add-stock", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_AddProductStock_InvalidQuantity_ShouldReturnBadRequest()
    {
        var (product, variant) = await SeedProductWithVariantAsync();
        var body = new { productVariantId = variant.Id, quantity = 0 };

        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/products/{product.Id}/add-stock", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_AddProductStock_IncompatibleUnits_ShouldReturnBadRequest()
    {
        // Gram (mass) variant against a Liter (volume) product → incompatible units
        var (product, variant) = await SeedProductWithVariantAsync(
            productUnit: UnitOfMeasure.Liter,
            variantUnit: UnitOfMeasure.Gram);
        var body = new { productVariantId = variant.Id, quantity = 1 };

        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/products/{product.Id}/add-stock", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Unit conversion happy paths ---------------------------------------------

    [Fact]
    public async Task POST_AddProductStock_GramVariantKilogramProduct_ShouldConvertAndReturnUpdatedStock()
    {
        // Variant: 500 g/bag; Product: kg; starting at 1 kg. Add 4 bags = 2000 g = 2 kg → 3 kg.
        var (product, variant) = await SeedProductWithVariantAsync(
            productStock: 1m,
            variantPackageSize: 500m,
            productUnit: UnitOfMeasure.Kilogram,
            variantUnit: UnitOfMeasure.Gram);
        var body = new { productVariantId = variant.Id, quantity = 4 };

        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/products/{product.Id}/add-stock", body);
        var responseBody = await response.Content.ReadFromJsonAsync<AddProductStockEndpoint.AddProductStockResponseBody>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3m, responseBody!.UpdatedProduct.Stock);
    }

    [Fact]
    public async Task POST_AddProductStock_MilliliterVariantLiterProduct_ShouldConvertAndReturnUpdatedStock()
    {
        // Variant: 500 ml/bottle; Product: L; starting at 1 L. Add 4 bottles = 2000 ml = 2 L → 3 L.
        var (product, variant) = await SeedProductWithVariantAsync(
            productStock: 1m,
            variantPackageSize: 500m,
            productUnit: UnitOfMeasure.Liter,
            variantUnit: UnitOfMeasure.Milliliter);
        var body = new { productVariantId = variant.Id, quantity = 4 };

        var response = await fixture.Client.PostAsJsonAsync($"/api/v1/products/{product.Id}/add-stock", body);
        var responseBody = await response.Content.ReadFromJsonAsync<AddProductStockEndpoint.AddProductStockResponseBody>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3m, responseBody!.UpdatedProduct.Stock);
    }
}

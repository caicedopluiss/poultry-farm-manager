using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;
using PoultryFarmManager.WebAPI.Endpoints.v1.Products;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class UpdateProductEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task PUT_Product_ValidRequest_ShouldReturnOk()
    {
        // Arrange - Create test product
        var product = new Product
        {
            Name = "Old Name",
            Manufacturer = "Old Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var updateProduct = new
        {
            Name = "Updated Name",
            Manufacturer = "Updated Manufacturer",
            UnitOfMeasure = nameof(UnitOfMeasure.Liter),
            Stock = 250.75m,
            Description = "Updated description"
        };
        var body = new { updateProduct };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/products/{product.Id}", body);
        var responseBody = await response.Content.ReadFromJsonAsync<UpdateProductEndpoint.UpdateProductResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Product);
        Assert.Equal(product.Id, responseBody.Product.Id);
        Assert.Equal("Updated Name", responseBody.Product.Name);
        Assert.Equal(nameof(UnitOfMeasure.Liter), responseBody.Product.UnitOfMeasure);
    }

    [Fact]
    public async Task PUT_Product_NonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var updateProduct = new
        {
            Name = "Updated Name",
            Manufacturer = "Updated Manufacturer",
            UnitOfMeasure = nameof(UnitOfMeasure.Kilogram),
            Stock = 100m,
            Description = ""
        };
        var body = new { updateProduct };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/products/{Guid.NewGuid()}", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

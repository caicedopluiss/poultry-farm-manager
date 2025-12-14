using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.WebAPI.Endpoints.v1.Products;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class CreateProductEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task POST_Products_ValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var newProduct = new
        {
            Name = "Premium Feed from API",
            Manufacturer = "FarmCo",
            UnitOfMeasure = nameof(UnitOfMeasure.Kilogram),
            Stock = 500.50m,
            Description = "High protein feed"
        };
        var body = new { newProduct };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/products", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreateProductEndpoint.CreateProductResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Product);
        Assert.Equal("Premium Feed from API", responseBody.Product.Name);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"/api/v1/products/{responseBody.Product.Id}", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task POST_Products_InvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange - Empty name
        var newProduct = new
        {
            Name = "",
            Manufacturer = "Test",
            UnitOfMeasure = nameof(UnitOfMeasure.Kilogram),
            Stock = 100m,
            Description = "Test"
        };
        var body = new { newProduct };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/products", body);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

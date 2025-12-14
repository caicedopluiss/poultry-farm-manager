using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.Products;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetProductByIdQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetProductByIdQuery.Args, GetProductByIdQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetProductByIdQuery.Args, GetProductByIdQuery.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetProductByIdQuery_ShouldReturnProduct_WhenExists()
    {
        // Arrange - Create a product
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 350.50m,
            Description = "Test product description"
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetProductByIdQuery.Args>(new(product.Id));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.Product);
        Assert.Equal(product.Id, result.Value!.Product.Id);
        Assert.Equal("Test Feed", result.Value!.Product.Name);
        Assert.Equal("Test Manufacturer", result.Value!.Product.Manufacturer);
        Assert.Equal(nameof(UnitOfMeasure.Kilogram), result.Value!.Product.UnitOfMeasure);
        Assert.Equal(350.50m, result.Value!.Product.Stock);
        Assert.Equal("Test product description", result.Value!.Product.Description);
    }

    [Fact]
    public async Task GetProductByIdQuery_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var request = new AppRequest<GetProductByIdQuery.Args>(new(Guid.NewGuid()));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value!.Product);
    }
}

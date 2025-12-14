using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.ProductVariants;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetProductVariantByIdQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetProductVariantByIdQuery.Args, GetProductVariantByIdQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetProductVariantByIdQuery.Args, GetProductVariantByIdQuery.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetProductVariantByIdQuery_ShouldReturnVariant_WhenExists()
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
            Stock = 50m,
            Quantity = 25,
            Description = "Standard 25kg feed bag"
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetProductVariantByIdQuery.Args>(new(variant.Id));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.ProductVariant);
        Assert.Equal(variant.Id, result.Value!.ProductVariant.Id);
        Assert.Equal("25kg Bag", result.Value!.ProductVariant.Name);
        Assert.Equal(product.Id, result.Value!.ProductVariant.ProductId);
        Assert.Equal(nameof(UnitOfMeasure.Kilogram), result.Value!.ProductVariant.UnitOfMeasure);
        Assert.Equal(50m, result.Value!.ProductVariant.Stock);
        Assert.Equal(25, result.Value!.ProductVariant.Quantity);
    }

    [Fact]
    public async Task GetProductVariantByIdQuery_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var request = new AppRequest<GetProductVariantByIdQuery.Args>(new(Guid.NewGuid()));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value!.ProductVariant);
    }
}

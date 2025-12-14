using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.ProductVariants;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetProductVariantsByProductIdQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetProductVariantsByProductIdQuery.Args, GetProductVariantsByProductIdQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetProductVariantsByProductIdQuery.Args, GetProductVariantsByProductIdQuery.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetProductVariantsByProductIdQuery_ShouldReturnVariantsForProduct()
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

        var request = new AppRequest<GetProductVariantsByProductIdQuery.Args>(new(product1.Id));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.ProductVariants.Count());
        Assert.All(result.Value!.ProductVariants, v => Assert.Equal(product1.Id, v.ProductId));
        Assert.Contains(result.Value!.ProductVariants, v => v.Name == "25kg Bag");
        Assert.Contains(result.Value!.ProductVariants, v => v.Name == "50kg Bag");
        Assert.DoesNotContain(result.Value!.ProductVariants, v => v.Name == "1L Bottle");
    }

    [Fact]
    public async Task GetProductVariantsByProductIdQuery_ShouldReturnEmptyList_WhenNoVariantsForProduct()
    {
        // Arrange - Create a product without variants
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "FarmCo",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetProductVariantsByProductIdQuery.Args>(new(product.Id));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.ProductVariants);
    }
}

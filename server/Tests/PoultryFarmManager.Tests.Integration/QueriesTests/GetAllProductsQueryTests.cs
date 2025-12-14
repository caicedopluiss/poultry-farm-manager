using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.Products;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetAllProductsQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetAllProductsQuery.Args, GetAllProductsQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetAllProductsQuery.Args, GetAllProductsQuery.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAllProductsQuery_ShouldReturnAllProducts()
    {
        // Arrange - Create multiple products
        var products = new[]
        {
            new Product
            {
                Name = "Premium Feed",
                Manufacturer = "FarmCo",
                UnitOfMeasure = UnitOfMeasure.Kilogram,
                Stock = 500m,
                Description = "High protein feed"
            },
            new Product
            {
                Name = "Starter Feed",
                Manufacturer = "AgriSupply",
                UnitOfMeasure = UnitOfMeasure.Kilogram,
                Stock = 200m
            },
            new Product
            {
                Name = "Vitamins",
                Manufacturer = "BioVet",
                UnitOfMeasure = UnitOfMeasure.Liter,
                Stock = 50m,
                Description = "Essential vitamins for poultry"
            }
        };
        dbContext.Products.AddRange(products);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetAllProductsQuery.Args>(new());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value!.Products.Count());
        Assert.Contains(result.Value!.Products, p => p.Name == "Premium Feed");
        Assert.Contains(result.Value!.Products, p => p.Name == "Starter Feed");
        Assert.Contains(result.Value!.Products, p => p.Name == "Vitamins");
    }

    [Fact]
    public async Task GetAllProductsQuery_ShouldReturnEmptyList_WhenNoProducts()
    {
        // Arrange
        var request = new AppRequest<GetAllProductsQuery.Args>(new());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.Products);
    }
}

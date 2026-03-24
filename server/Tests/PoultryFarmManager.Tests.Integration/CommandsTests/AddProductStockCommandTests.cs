using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.Products;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class AddProductStockCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<AddProductStockCommand.Args, AddProductStockCommand.Result> handler =
        fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<AddProductStockCommand.Args, AddProductStockCommand.Result>>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    // Helpers -----------------------------------------------------------------

    private async Task<(Product product, ProductVariant variant)> SeedProductWithVariantAsync(
        decimal productStock = 100m,
        decimal variantPackageSize = 25m,
        UnitOfMeasure unitOfMeasure = UnitOfMeasure.Kilogram,
        UnitOfMeasure? variantUnitOfMeasure = null)
    {
        var product = new Product
        {
            Name = $"Feed_{Guid.NewGuid().ToString()[..8]}",
            Manufacturer = "TestCo",
            UnitOfMeasure = unitOfMeasure,
            Stock = productStock
        };
        dbContext.Products.Add(product);

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Bag",
            UnitOfMeasure = variantUnitOfMeasure ?? unitOfMeasure,
            Stock = variantPackageSize   // package size (e.g. 25 kg per bag)
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        return (product, variant);
    }

    // Happy path --------------------------------------------------------------

    [Fact]
    public async Task AddProductStockCommand_ShouldIncreaseProductStock_WhenValid()
    {
        // Arrange: product has 100 kg, variant = 25 kg/bag, add 4 bags → +100 kg
        var (product, variant) = await SeedProductWithVariantAsync(productStock: 100m, variantPackageSize: 25m);
        var request = new AppRequest<AddProductStockCommand.Args>(new(product.Id, variant.Id, Quantity: 4));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(200m, result.Value!.UpdatedProduct.Stock);   // 100 + 4*25
    }

    [Fact]
    public async Task AddProductStockCommand_ShouldNotChangeVariantStock()
    {
        // Arrange
        var (product, variant) = await SeedProductWithVariantAsync(variantPackageSize: 25m);
        var request = new AppRequest<AddProductStockCommand.Args>(new(product.Id, variant.Id, Quantity: 3));

        // Act
        await handler.HandleAsync(request, CancellationToken.None);

        // Assert: variant's package-size must be untouched in the DB
        var variantInDb = await dbContext.ProductVariants.AsNoTracking().FirstAsync(v => v.Id == variant.Id);
        Assert.Equal(25m, variantInDb.Stock);
    }

    // Validation errors -------------------------------------------------------

    [Fact]
    public async Task AddProductStockCommand_ShouldReturnValidationError_WhenProductIdIsEmpty()
    {
        var request = new AppRequest<AddProductStockCommand.Args>(new(Guid.Empty, Guid.NewGuid(), Quantity: 1));

        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "productId");
    }

    [Fact]
    public async Task AddProductStockCommand_ShouldReturnValidationError_WhenProductVariantIdIsEmpty()
    {
        var request = new AppRequest<AddProductStockCommand.Args>(new(Guid.NewGuid(), Guid.Empty, Quantity: 1));

        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "productVariantId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task AddProductStockCommand_ShouldReturnValidationError_WhenQuantityIsInvalid(int quantity)
    {
        var (product, variant) = await SeedProductWithVariantAsync();
        var request = new AppRequest<AddProductStockCommand.Args>(new(product.Id, variant.Id, quantity));

        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "quantity");
    }

    [Fact]
    public async Task AddProductStockCommand_ShouldReturnValidationError_WhenProductNotFound()
    {
        var (_, variant) = await SeedProductWithVariantAsync();
        var request = new AppRequest<AddProductStockCommand.Args>(new(Guid.NewGuid(), variant.Id, Quantity: 1));

        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "productId" && e.error.Contains("not found"));
    }

    [Fact]
    public async Task AddProductStockCommand_ShouldReturnValidationError_WhenVariantNotFound()
    {
        var (product, _) = await SeedProductWithVariantAsync();
        var request = new AppRequest<AddProductStockCommand.Args>(new(product.Id, Guid.NewGuid(), Quantity: 1));

        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "productVariantId" && e.error.Contains("not found"));
    }

    [Fact]
    public async Task AddProductStockCommand_ShouldReturnValidationError_WhenVariantDoesNotBelongToProduct()
    {
        // Arrange: two separate products, try to use variant from product2 with product1
        var (product1, _) = await SeedProductWithVariantAsync();
        var (_, variant2) = await SeedProductWithVariantAsync();

        var request = new AppRequest<AddProductStockCommand.Args>(new(product1.Id, variant2.Id, Quantity: 1));

        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "productVariantId");
    }

    // Unit conversion ---------------------------------------------------------

    [Fact]
    public async Task AddProductStockCommand_ShouldConvertGramsToKilograms_WhenProductIsInKilograms()
    {
        // Variant: 500 g/bag; Product: Kilograms. Add 4 bags = 2000 g = 2 kg.
        // Product starts at 1 kg → should end at 3 kg.
        var (product, variant) = await SeedProductWithVariantAsync(
            productStock: 1m,
            variantPackageSize: 500m,
            unitOfMeasure: UnitOfMeasure.Kilogram,
            variantUnitOfMeasure: UnitOfMeasure.Gram);

        var request = new AppRequest<AddProductStockCommand.Args>(new(product.Id, variant.Id, Quantity: 4));
        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        // 4 bags × 500 g = 2000 g ÷ 1000 = 2 kg added → 1 + 2 = 3 kg
        Assert.Equal(3m, result.Value!.UpdatedProduct.Stock);
    }

    [Fact]
    public async Task AddProductStockCommand_ShouldConvertKilogramsToGrams_WhenProductIsInGrams()
    {
        // Variant: 0.5 kg/bag; Product: Grams. Add 2 bags = 1 kg = 1000 g.
        // Product starts at 1000 g → should end at 2000 g.
        var (product, variant) = await SeedProductWithVariantAsync(
            productStock: 1000m,
            variantPackageSize: 0.5m,
            unitOfMeasure: UnitOfMeasure.Gram,
            variantUnitOfMeasure: UnitOfMeasure.Kilogram);

        var request = new AppRequest<AddProductStockCommand.Args>(new(product.Id, variant.Id, Quantity: 2));
        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        // 2 bags × 0.5 kg = 1 kg × 1000 = 1000 g added → 1000 + 1000 = 2000 g
        Assert.Equal(2000m, result.Value!.UpdatedProduct.Stock);
    }

    [Fact]
    public async Task AddProductStockCommand_ShouldConvertMillilitersToLiters_WhenProductIsInLiters()
    {
        // Variant: 500 ml/bottle; Product: Liters. Add 4 bottles = 2000 ml = 2 L.
        // Product starts at 1 L → should end at 3 L.
        var (product, variant) = await SeedProductWithVariantAsync(
            productStock: 1m,
            variantPackageSize: 500m,
            unitOfMeasure: UnitOfMeasure.Liter,
            variantUnitOfMeasure: UnitOfMeasure.Milliliter);

        var request = new AppRequest<AddProductStockCommand.Args>(new(product.Id, variant.Id, Quantity: 4));
        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        // 4 bottles × 500 ml = 2000 ml ÷ 1000 = 2 L added → 1 + 2 = 3 L
        Assert.Equal(3m, result.Value!.UpdatedProduct.Stock);
    }

    [Fact]
    public async Task AddProductStockCommand_ShouldReturnValidationError_WhenVariantAndProductUnitsAreIncompatible()
    {
        // Gram (mass) variant cannot be converted to Liter (volume) product → validation error.
        var (product, variant) = await SeedProductWithVariantAsync(
            productStock: 5m,
            variantPackageSize: 100m,
            unitOfMeasure: UnitOfMeasure.Liter,
            variantUnitOfMeasure: UnitOfMeasure.Gram);

        var request = new AppRequest<AddProductStockCommand.Args>(new(product.Id, variant.Id, Quantity: 3));
        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "productVariantId");
    }
}

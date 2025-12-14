using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.ProductVariants;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class CreateProductVariantCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<CreateProductVariantCommand.Args, CreateProductVariantCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<CreateProductVariantCommand.Args, CreateProductVariantCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateProductVariantCommand_ShouldCreateVariant_WithValidData()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var newVariantDto = new NewProductVariantDto(
            ProductId: product.Id,
            Name: "25kg Bag",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 100m,
            Quantity: 25,
            Description: "25 kilogram bag"
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.CreatedProductVariant.Id);
        Assert.Equal(product.Id, result.Value!.CreatedProductVariant.ProductId);
        Assert.Equal("25kg Bag", result.Value!.CreatedProductVariant.Name);
        Assert.Equal(nameof(UnitOfMeasure.Kilogram), result.Value!.CreatedProductVariant.UnitOfMeasure);
        Assert.Equal(2600m, result.Value!.CreatedProductVariant.Stock); // 100 + (100 * 25) converted
        Assert.Equal(25, result.Value!.CreatedProductVariant.Quantity);

        var variantInDb = await dbContext.ProductVariants.FindAsync(result.Value!.CreatedProductVariant.Id);
        Assert.NotNull(variantInDb);
        Assert.Equal("25kg Bag", variantInDb.Name);
    }

    [Fact]
    public async Task CreateProductVariantCommand_ShouldFail_WithEmptyName()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var newVariantDto = new NewProductVariantDto(
            ProductId: product.Id,
            Name: "",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 100m,
            Quantity: 25,
            Description: "Test"
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateProductVariantCommand_ShouldFail_WithInvalidProductId()
    {
        // Arrange
        var newVariantDto = new NewProductVariantDto(
            ProductId: Guid.NewGuid(),
            Name: "Test Variant",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 100m,
            Quantity: 25,
            Description: "Test"
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task CreateProductVariantCommand_ShouldFail_WithNegativeStock()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var newVariantDto = new NewProductVariantDto(
            ProductId: product.Id,
            Name: "Test Variant",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: -10m,
            Quantity: 25,
            Description: "Test"
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateProductVariantCommand_ShouldFail_WithNameExceedingMaxLength()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var newVariantDto = new NewProductVariantDto(
            ProductId: product.Id,
            Name: new string('A', 101), // 101 characters - exceeds max length of 100
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 100m,
            Quantity: 25,
            Description: "Valid description"
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateProductVariantCommand_ShouldFail_WithDescriptionExceedingMaxLength()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var newVariantDto = new NewProductVariantDto(
            ProductId: product.Id,
            Name: "Valid Name",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 100m,
            Quantity: 25,
            Description: new string('B', 501) // 501 characters - exceeds max length of 500
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "description");
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateProductVariantCommand_ShouldFail_WithZeroQuantity()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var newVariantDto = new NewProductVariantDto(
            ProductId: product.Id,
            Name: "Valid Name",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 100m,
            Quantity: 0, // Zero quantity - should be greater than zero
            Description: "Valid description"
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "quantity");
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateProductVariantCommand_ShouldFail_WithNegativeQuantity()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var newVariantDto = new NewProductVariantDto(
            ProductId: product.Id,
            Name: "Valid Name",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 100m,
            Quantity: -5, // Negative quantity - should be greater than zero
            Description: "Valid description"
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "quantity");
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateProductVariantCommand_ShouldFail_WithInvalidUnitOfMeasure()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var newVariantDto = new NewProductVariantDto(
            ProductId: product.Id,
            Name: "Valid Name",
            UnitOfMeasure: "InvalidUnit",
            Stock: 100m,
            Quantity: 25,
            Description: "Valid description"
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "unitOfMeasure");
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateProductVariantCommand_ShouldFail_WithEmptyUnitOfMeasure()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var newVariantDto = new NewProductVariantDto(
            ProductId: product.Id,
            Name: "Valid Name",
            UnitOfMeasure: "", // Empty unit of measure
            Stock: 100m,
            Quantity: 25,
            Description: "Valid description"
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "unitOfMeasure");
    }

    [Fact]
    public async Task CreateProductVariantCommand_ShouldFail_WithMultipleValidationErrors()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var newVariantDto = new NewProductVariantDto(
            ProductId: product.Id,
            Name: "", // Empty name
            UnitOfMeasure: "InvalidUnit", // Invalid unit
            Stock: -50m, // Negative stock
            Quantity: 0, // Zero quantity
            Description: new string('B', 501) // Description too long
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
        Assert.Contains(result.ValidationErrors, e => e.field == "unitOfMeasure");
        Assert.Contains(result.ValidationErrors, e => e.field == "stock");
        Assert.Contains(result.ValidationErrors, e => e.field == "quantity");
        Assert.Contains(result.ValidationErrors, e => e.field == "description");
        Assert.Equal(5, result.ValidationErrors.Count());
    }
}

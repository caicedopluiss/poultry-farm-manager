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

public class UpdateProductVariantCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<UpdateProductVariantCommand.Args, UpdateProductVariantCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<UpdateProductVariantCommand.Args, UpdateProductVariantCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UpdateProductVariantCommand_ShouldUpdateVariant_WithValidData()
    {
        // Arrange - Create a product and variant first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Old Variant Name",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m,
            Quantity = 10,
            Description = "Old description"
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductVariantDto(
            Name: "Updated Variant Name",
            UnitOfMeasure: nameof(UnitOfMeasure.Gram),
            Stock: 250m,
            Quantity: 20,
            Description: "Updated description"
        );
        var request = new AppRequest<UpdateProductVariantCommand.Args>(new(variant.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(variant.Id, result.Value!.UpdatedProductVariant.Id);
        Assert.Equal("Updated Variant Name", result.Value!.UpdatedProductVariant.Name);
        Assert.Equal(nameof(UnitOfMeasure.Gram), result.Value!.UpdatedProductVariant.UnitOfMeasure);
        Assert.Equal(250m, result.Value!.UpdatedProductVariant.Stock);
        Assert.Equal(20, result.Value!.UpdatedProductVariant.Quantity);
    }

    [Fact]
    public async Task UpdateProductVariantCommand_ShouldFail_WithNonExistentId()
    {
        // Arrange
        var updateDto = new UpdateProductVariantDto(
            Name: "Updated Name",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 100m,
            Quantity: 10,
            Description: null
        );
        var request = new AppRequest<UpdateProductVariantCommand.Args>(new(Guid.NewGuid(), updateDto));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task UpdateProductVariantCommand_ShouldFail_WithNegativeStock()
    {
        // Arrange - Create a product and variant first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Test Variant",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m,
            Quantity = 10
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductVariantDto(
            Name: "Test Variant",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: -50m,
            Quantity: 10,
            Description: null
        );
        var request = new AppRequest<UpdateProductVariantCommand.Args>(new(variant.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationErrors);
    }

    [Fact]
    public async Task UpdateProductVariantCommand_ShouldFail_WithNameExceedingMaxLength()
    {
        // Arrange - Create a product and variant first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Original Variant",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m,
            Quantity = 10
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductVariantDto(
            Name: new string('A', 101), // 101 characters - exceeds max length of 100
            UnitOfMeasure: null,
            Stock: null,
            Quantity: null,
            Description: null
        );
        var request = new AppRequest<UpdateProductVariantCommand.Args>(new(variant.Id, updateDto));

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
    public async Task UpdateProductVariantCommand_ShouldFail_WithDescriptionExceedingMaxLength()
    {
        // Arrange - Create a product and variant first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Original Variant",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m,
            Quantity = 10
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductVariantDto(
            Name: null,
            UnitOfMeasure: null,
            Stock: null,
            Quantity: null,
            Description: new string('B', 501) // 501 characters - exceeds max length of 500
        );
        var request = new AppRequest<UpdateProductVariantCommand.Args>(new(variant.Id, updateDto));

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
    public async Task UpdateProductVariantCommand_ShouldFail_WithInvalidUnitOfMeasure()
    {
        // Arrange - Create a product and variant first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Original Variant",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m,
            Quantity = 10
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductVariantDto(
            Name: null,
            UnitOfMeasure: "InvalidUnit",
            Stock: null,
            Quantity: null,
            Description: null
        );
        var request = new AppRequest<UpdateProductVariantCommand.Args>(new(variant.Id, updateDto));

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
    public async Task UpdateProductVariantCommand_ShouldFail_WithZeroQuantity()
    {
        // Arrange - Create a product and variant first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Original Variant",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m,
            Quantity = 10
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductVariantDto(
            Name: null,
            UnitOfMeasure: null,
            Stock: null,
            Quantity: 0, // Zero quantity - should be greater than zero
            Description: null
        );
        var request = new AppRequest<UpdateProductVariantCommand.Args>(new(variant.Id, updateDto));

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
    public async Task UpdateProductVariantCommand_ShouldFail_WithNegativeQuantity()
    {
        // Arrange - Create a product and variant first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Original Variant",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m,
            Quantity = 10
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductVariantDto(
            Name: null,
            UnitOfMeasure: null,
            Stock: null,
            Quantity: -5, // Negative quantity - should be greater than zero
            Description: null
        );
        var request = new AppRequest<UpdateProductVariantCommand.Args>(new(variant.Id, updateDto));

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
    public async Task UpdateProductVariantCommand_ShouldFail_WithMultipleValidationErrors()
    {
        // Arrange - Create a product and variant first
        var product = new Product
        {
            Name = "Test Feed",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 1000m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Name = "Original Variant",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m,
            Quantity = 10
        };
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductVariantDto(
            Name: new string('A', 101), // Name too long
            UnitOfMeasure: "InvalidUnit", // Invalid unit
            Stock: -100m, // Negative stock
            Quantity: 0, // Zero quantity
            Description: new string('B', 501) // Description too long
        );
        var request = new AppRequest<UpdateProductVariantCommand.Args>(new(variant.Id, updateDto));

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

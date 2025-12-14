using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.Products;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class UpdateProductCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<UpdateProductCommand.Args, UpdateProductCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<UpdateProductCommand.Args, UpdateProductCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UpdateProductCommand_ShouldUpdateProduct_WithValidData()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Old Product Name",
            Manufacturer = "Old Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m,
            Description = "Old description"
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductDto(
            Name: "Updated Product Name",
            Manufacturer: "Updated Manufacturer",
            UnitOfMeasure: nameof(UnitOfMeasure.Liter),
            Stock: 250.75m,
            Description: "Updated description"
        );
        var request = new AppRequest<UpdateProductCommand.Args>(new(product.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(product.Id, result.Value!.UpdatedProduct.Id);
        Assert.Equal("Updated Product Name", result.Value!.UpdatedProduct.Name);
        Assert.Equal("Updated Manufacturer", result.Value!.UpdatedProduct.Manufacturer);
        Assert.Equal(nameof(UnitOfMeasure.Liter), result.Value!.UpdatedProduct.UnitOfMeasure);
        Assert.Equal(250.75m, result.Value!.UpdatedProduct.Stock);
    }

    [Fact]
    public async Task UpdateProductCommand_ShouldFail_WithNonExistentId()
    {
        // Arrange
        var updateDto = new UpdateProductDto(
            Name: "Updated Name",
            Manufacturer: "Updated Manufacturer",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 100m,
            Description: null
        );
        var request = new AppRequest<UpdateProductCommand.Args>(new(Guid.NewGuid(), updateDto));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task UpdateProductCommand_ShouldFail_WithNegativeStock()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Product",
            Manufacturer = "Test Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductDto(
            Name: "Test Product",
            Manufacturer: "Test Manufacturer",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: -50m,
            Description: null
        );
        var request = new AppRequest<UpdateProductCommand.Args>(new(product.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationErrors);
    }

    [Fact]
    public async Task UpdateProductCommand_ShouldFail_WithNameExceedingMaxLength()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Original Product",
            Manufacturer = "Original Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductDto(
            Name: new string('A', 101), // 101 characters - exceeds max length of 100
            Manufacturer: null,
            UnitOfMeasure: null,
            Stock: null,
            Description: null
        );
        var request = new AppRequest<UpdateProductCommand.Args>(new(product.Id, updateDto));

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
    public async Task UpdateProductCommand_ShouldFail_WithManufacturerExceedingMaxLength()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Original Product",
            Manufacturer = "Original Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductDto(
            Name: null,
            Manufacturer: new string('B', 101), // 101 characters - exceeds max length of 100
            UnitOfMeasure: null,
            Stock: null,
            Description: null
        );
        var request = new AppRequest<UpdateProductCommand.Args>(new(product.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "manufacturer");
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task UpdateProductCommand_ShouldFail_WithDescriptionExceedingMaxLength()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Original Product",
            Manufacturer = "Original Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductDto(
            Name: null,
            Manufacturer: null,
            UnitOfMeasure: null,
            Stock: null,
            Description: new string('C', 501) // 501 characters - exceeds max length of 500
        );
        var request = new AppRequest<UpdateProductCommand.Args>(new(product.Id, updateDto));

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
    public async Task UpdateProductCommand_ShouldFail_WithInvalidUnitOfMeasure()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Original Product",
            Manufacturer = "Original Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductDto(
            Name: null,
            Manufacturer: null,
            UnitOfMeasure: "InvalidUnit",
            Stock: null,
            Description: null
        );
        var request = new AppRequest<UpdateProductCommand.Args>(new(product.Id, updateDto));

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
    public async Task UpdateProductCommand_ShouldFail_WithMultipleValidationErrors()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Original Product",
            Manufacturer = "Original Manufacturer",
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Stock = 100m
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateProductDto(
            Name: new string('A', 101), // Name too long
            Manufacturer: new string('B', 101), // Manufacturer too long
            UnitOfMeasure: "InvalidUnit", // Invalid unit
            Stock: -100m, // Negative stock
            Description: new string('C', 501) // Description too long
        );
        var request = new AppRequest<UpdateProductCommand.Args>(new(product.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
        Assert.Contains(result.ValidationErrors, e => e.field == "manufacturer");
        Assert.Contains(result.ValidationErrors, e => e.field == "unitOfMeasure");
        Assert.Contains(result.ValidationErrors, e => e.field == "stock");
        Assert.Contains(result.ValidationErrors, e => e.field == "description");
        Assert.Equal(5, result.ValidationErrors.Count());
    }
}

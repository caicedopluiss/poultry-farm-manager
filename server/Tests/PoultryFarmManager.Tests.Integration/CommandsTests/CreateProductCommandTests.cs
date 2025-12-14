using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.Products;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class CreateProductCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<CreateProductCommand.Args, CreateProductCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<CreateProductCommand.Args, CreateProductCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateProductCommand_ShouldCreateProduct_WithValidData()
    {
        // Arrange
        var newProductDto = new NewProductDto(
            Name: "Premium Feed",
            Manufacturer: "FarmCo",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 500.50m,
            Description: "High protein feed"
        );
        var request = new AppRequest<CreateProductCommand.Args>(new(newProductDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.CreatedProduct.Id);
        Assert.Equal("Premium Feed", result.Value!.CreatedProduct.Name);
        Assert.Equal("FarmCo", result.Value!.CreatedProduct.Manufacturer);
        Assert.Equal(nameof(UnitOfMeasure.Kilogram), result.Value!.CreatedProduct.UnitOfMeasure);
        Assert.Equal(500.50m, result.Value!.CreatedProduct.Stock);

        var productInDb = await dbContext.Products.FindAsync(result.Value!.CreatedProduct.Id);
        Assert.NotNull(productInDb);
        Assert.Equal("Premium Feed", productInDb.Name);
    }

    [Fact]
    public async Task CreateProductCommand_ShouldFail_WithEmptyName()
    {
        // Arrange
        var newProductDto = new NewProductDto(
            Name: "",
            Manufacturer: "Test",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 100m,
            Description: "Test"
        );
        var request = new AppRequest<CreateProductCommand.Args>(new(newProductDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateProductCommand_ShouldFail_WithNegativeStock()
    {
        // Arrange
        var newProductDto = new NewProductDto(
            Name: "Test Product",
            Manufacturer: "Test",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: -10m,
            Description: "Test"
        );
        var request = new AppRequest<CreateProductCommand.Args>(new(newProductDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateProductCommand_ShouldFail_WithInvalidUnitOfMeasure()
    {
        // Arrange
        var newProductDto = new NewProductDto(
            Name: "Test Product",
            Manufacturer: "Test",
            UnitOfMeasure: "InvalidUnit",
            Stock: 100m,
            Description: "Test"
        );
        var request = new AppRequest<CreateProductCommand.Args>(new(newProductDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateProductCommand_ShouldFail_WithNameExceedingMaxLength()
    {
        // Arrange
        var newProductDto = new NewProductDto(
            Name: new string('A', 101), // 101 characters - exceeds max length of 100
            Manufacturer: "Valid Manufacturer",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 100m,
            Description: "Valid description"
        );
        var request = new AppRequest<CreateProductCommand.Args>(new(newProductDto));

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
    public async Task CreateProductCommand_ShouldFail_WithEmptyManufacturer()
    {
        // Arrange
        var newProductDto = new NewProductDto(
            Name: "Valid Product",
            Manufacturer: "", // Empty manufacturer
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 100m,
            Description: "Valid description"
        );
        var request = new AppRequest<CreateProductCommand.Args>(new(newProductDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "manufacturer");
    }

    [Fact]
    public async Task CreateProductCommand_ShouldFail_WithManufacturerExceedingMaxLength()
    {
        // Arrange
        var newProductDto = new NewProductDto(
            Name: "Valid Product",
            Manufacturer: new string('B', 101), // 101 characters - exceeds max length of 100
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 100m,
            Description: "Valid description"
        );
        var request = new AppRequest<CreateProductCommand.Args>(new(newProductDto));

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
    public async Task CreateProductCommand_ShouldFail_WithDescriptionExceedingMaxLength()
    {
        // Arrange
        var newProductDto = new NewProductDto(
            Name: "Valid Product",
            Manufacturer: "Valid Manufacturer",
            UnitOfMeasure: nameof(UnitOfMeasure.Kilogram),
            Stock: 100m,
            Description: new string('C', 501) // 501 characters - exceeds max length of 500
        );
        var request = new AppRequest<CreateProductCommand.Args>(new(newProductDto));

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
    public async Task CreateProductCommand_ShouldFail_WithEmptyUnitOfMeasure()
    {
        // Arrange
        var newProductDto = new NewProductDto(
            Name: "Valid Product",
            Manufacturer: "Valid Manufacturer",
            UnitOfMeasure: "", // Empty unit of measure
            Stock: 100m,
            Description: "Valid description"
        );
        var request = new AppRequest<CreateProductCommand.Args>(new(newProductDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "unitOfMeasure");
    }

    [Fact]
    public async Task CreateProductCommand_ShouldFail_WithMultipleValidationErrors()
    {
        // Arrange
        var newProductDto = new NewProductDto(
            Name: "", // Empty name
            Manufacturer: new string('B', 101), // Manufacturer too long
            UnitOfMeasure: "InvalidUnit", // Invalid unit
            Stock: -50m, // Negative stock
            Description: new string('C', 501) // Description too long
        );
        var request = new AppRequest<CreateProductCommand.Args>(new(newProductDto));

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

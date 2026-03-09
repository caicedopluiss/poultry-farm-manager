using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.ProductVariants;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.Core.Models.Finance;
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
        // Arrange - Create a product and vendor first
        var person = new Person
        {
            FirstName = "John",
            LastName = "Supplier",
            Email = "john@supplier.com"
        };
        dbContext.Persons.Add(person);

        var vendor = new Vendor
        {
            Name = "Test Vendor",
            ContactPersonId = person.Id
        };
        dbContext.Vendors.Add(vendor);

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
            Description: "25 kilogram bag",
            VendorId: vendor.Id,
            UnitPrice: 15.50m
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
        Assert.Equal(100m, result.Value!.CreatedProductVariant.Stock);

        var variantInDb = await dbContext.ProductVariants.FindAsync(result.Value!.CreatedProductVariant.Id);
        Assert.NotNull(variantInDb);
        Assert.Equal("25kg Bag", variantInDb.Name);

        // Verify transaction was created
        var transaction = dbContext.Transactions
            .FirstOrDefault(t => t.ProductVariantId == variantInDb.Id);
        Assert.NotNull(transaction);
        Assert.Equal(vendor.Id, transaction.VendorId);
        Assert.Equal(15.50m, transaction.UnitPrice);
        Assert.Null(transaction.Quantity);
        Assert.Equal(15.50m, transaction.TransactionAmount);
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
            Description: "Test",
            VendorId: Guid.NewGuid(),
            UnitPrice: 15.00m
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
            Description: "Test",
            VendorId: Guid.NewGuid(),
            UnitPrice: 15.00m
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "vendorId");
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
            Description: "Test",
            VendorId: Guid.NewGuid(),
            UnitPrice: 15.00m
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
            Description: "Valid description",
            VendorId: Guid.NewGuid(),
            UnitPrice: 15.00m
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
        Assert.Contains(result.ValidationErrors, e => e.field == "vendorId");
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
            Description: new string('B', 501), // 501 characters - exceeds max length of 500
            VendorId: Guid.NewGuid(),
            UnitPrice: 15.00m
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "description");
        Assert.Contains(result.ValidationErrors, e => e.field == "vendorId");
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
            Description: "Valid description",
            VendorId: Guid.NewGuid(),
            UnitPrice: 15.00m
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
            Description: "Valid description",
            VendorId: Guid.NewGuid(),
            UnitPrice: 15.00m
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
            Description: new string('B', 501), // Description too long
            VendorId: Guid.Empty,
            UnitPrice: -10.00m
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
        Assert.Contains(result.ValidationErrors, e => e.field == "description");
        Assert.Contains(result.ValidationErrors, e => e.field == "vendorId");
        Assert.Contains(result.ValidationErrors, e => e.field == "unitPrice");
        Assert.Equal(6, result.ValidationErrors.Count());
    }

    [Fact]
    public async Task CreateProductVariantCommand_ShouldFail_WithEmptyVendorId()
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
            Description: "Valid description",
            VendorId: Guid.Empty, // Empty vendor ID
            UnitPrice: 15.00m
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "vendorId");
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateProductVariantCommand_ShouldFail_WithInvalidVendorId()
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
            Description: "Valid description",
            VendorId: Guid.NewGuid(), // Vendor doesn't exist in database
            UnitPrice: 15.00m
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "vendorId" && e.error.Contains("not found"));
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateProductVariantCommand_ShouldFail_WithZeroUnitPrice()
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
            Description: "Valid description",
            VendorId: Guid.NewGuid(),
            UnitPrice: 0m // Zero unit price
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "unitPrice");
        Assert.True(result.ValidationErrors.Count() >= 1); // May also have vendor validation error
    }

    [Fact]
    public async Task CreateProductVariantCommand_ShouldFail_WithNegativeUnitPrice()
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
            Description: "Valid description",
            VendorId: Guid.NewGuid(),
            UnitPrice: -10.50m // Negative unit price
        );
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(newVariantDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "unitPrice");
        Assert.True(result.ValidationErrors.Count() >= 1); // May also have vendor validation error
    }
}

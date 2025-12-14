using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Commands.Batches;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class RegisterProductConsumptionCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<RegisterProductConsumptionCommand.Args, RegisterProductConsumptionCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<RegisterProductConsumptionCommand.Args, RegisterProductConsumptionCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RegisterProductConsumptionCommand_ShouldRegisterConsumption_AndReduceProductStock()
    {
        // Arrange - Create a batch and product
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Product Consumption",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = BatchStatus.Active,
            Shed = "Shed A-1"
        };
        dbContext.Batches.Add(batch);

        var product = new Core.Models.Inventory.Product
        {
            Name = "Chicken Feed Premium",
            Manufacturer = "FeedCo",
            Stock = 1000m, // 1000 kg
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Description = "High quality feed"
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var consumption = new NewProductConsumptionDto(
            ProductId: product.Id,
            Stock: 50m, // 50 kg
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Daily feeding"
        );
        var request = new AppRequest<RegisterProductConsumptionCommand.Args>(new(batch.Id, consumption));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Detach entities to reload fresh from DB
        dbContext.Entry(batch).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        dbContext.Entry(product).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

        var updatedProduct = await dbContext.Products.FindAsync(product.Id);
        var consumptionRecord = await dbContext.ProductConsumptionActivities.FindAsync(result.Value!.ProductConsumption.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.ProductConsumption.Id);
        Assert.Equal(batch.Id, result.Value!.ProductConsumption.BatchId);
        Assert.Equal(product.Id, result.Value!.ProductConsumption.ProductId);
        Assert.Equal(50m, result.Value!.ProductConsumption.Stock);
        Assert.Equal("Kilogram", result.Value!.ProductConsumption.UnitOfMeasure);
        Assert.Equal("Daily feeding", result.Value!.ProductConsumption.Notes);

        // Verify product stock was reduced
        Assert.NotNull(updatedProduct);
        Assert.Equal(950m, updatedProduct!.Stock); // 1000 - 50 = 950

        Assert.NotNull(consumptionRecord);
        Assert.Equal(product.Id, consumptionRecord!.ProductId);
        Assert.Equal(50m, consumptionRecord.Stock);
        Assert.Equal(UnitOfMeasure.Kilogram, consumptionRecord.UnitOfMeasure);
    }

    [Fact]
    public async Task RegisterProductConsumptionCommand_WithUnitConversion_ShouldConvertAndReduceStock()
    {
        // Arrange - Product in Kilograms, consumption in Grams
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Unit Conversion",
            Breed = "Layer",
            StartDate = DateTime.UtcNow,
            MaleCount = 50,
            FemaleCount = 150,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = BatchStatus.Active,
            Shed = "Shed B-2"
        };
        dbContext.Batches.Add(batch);

        var product = new Core.Models.Inventory.Product
        {
            Name = "Vitamin Supplement",
            Manufacturer = "BioVet",
            Stock = 10m, // 10 kg
            UnitOfMeasure = UnitOfMeasure.Kilogram,
            Description = "Essential vitamins"
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var consumption = new NewProductConsumptionDto(
            ProductId: product.Id,
            Stock: 2500m, // 2500 grams = 2.5 kg
            UnitOfMeasure: "Gram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Weekly vitamin dose"
        );
        var request = new AppRequest<RegisterProductConsumptionCommand.Args>(new(batch.Id, consumption));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Detach entities to reload fresh from DB
        dbContext.Entry(product).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        var updatedProduct = await dbContext.Products.FindAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(2500m, result.Value!.ProductConsumption.Stock);
        Assert.Equal("Gram", result.Value!.ProductConsumption.UnitOfMeasure);

        // Verify stock was reduced by converted amount (2.5 kg)
        Assert.NotNull(updatedProduct);
        Assert.Equal(7.5m, updatedProduct!.Stock); // 10 - 2.5 = 7.5 kg
    }

    [Fact]
    public async Task RegisterProductConsumptionCommand_WithMultipleConsumptions_ShouldReduceStockCorrectly()
    {
        // Arrange - Create batch and product
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Multiple Consumptions",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 150,
            FemaleCount = 150,
            UnsexedCount = 0,
            InitialPopulation = 300,
            Status = BatchStatus.Active,
            Shed = "Shed C-1"
        };
        dbContext.Batches.Add(batch);

        var product = new Core.Models.Inventory.Product
        {
            Name = "Standard Feed",
            Manufacturer = "FarmCo",
            Stock = 500m,
            UnitOfMeasure = UnitOfMeasure.Kilogram
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        // First consumption
        var consumption1 = new NewProductConsumptionDto(
            ProductId: product.Id,
            Stock: 100m,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Day 1"
        );
        await handler.HandleAsync(new AppRequest<RegisterProductConsumptionCommand.Args>(new(batch.Id, consumption1)), CancellationToken.None);

        // Second consumption
        var consumption2 = new NewProductConsumptionDto(
            ProductId: product.Id,
            Stock: 75m,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Day 2"
        );
        var result = await handler.HandleAsync(new AppRequest<RegisterProductConsumptionCommand.Args>(new(batch.Id, consumption2)), CancellationToken.None);

        // Detach and reload
        dbContext.Entry(product).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        var updatedProduct = await dbContext.Products.FindAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(updatedProduct);
        Assert.Equal(325m, updatedProduct!.Stock); // 500 - 100 - 75 = 325
    }

    [Fact]
    public async Task RegisterProductConsumptionCommand_ShouldThrowException_WhenBatchDoesNotExist()
    {
        // Arrange
        var nonExistentBatchId = Guid.NewGuid();
        var product = new Core.Models.Inventory.Product
        {
            Name = "Test Product",
            Manufacturer = "TestCo",
            Stock = 100m,
            UnitOfMeasure = UnitOfMeasure.Kilogram
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var consumption = new NewProductConsumptionDto(
            ProductId: product.Id,
            Stock: 10m,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterProductConsumptionCommand.Args>(new(nonExistentBatchId, consumption));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(request, CancellationToken.None)
        );

        Assert.Contains("not found", exception.Message);
        Assert.Contains(nonExistentBatchId.ToString(), exception.Message);
    }

    [Fact]
    public async Task RegisterProductConsumptionCommand_ShouldReturnValidationError_WhenProductDoesNotExist()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = BatchStatus.Active,
            Shed = "Shed A-1"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var nonExistentProductId = Guid.NewGuid();
        var consumption = new NewProductConsumptionDto(
            ProductId: nonExistentProductId,
            Stock: 10m,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterProductConsumptionCommand.Args>(new(batch.Id, consumption));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "productId" && e.error.Contains("not found"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public async Task RegisterProductConsumptionCommand_ShouldReturnValidationError_ForInvalidStock(decimal stock)
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Invalid Stock",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = BatchStatus.Active,
            Shed = "Shed A-1"
        };
        dbContext.Batches.Add(batch);

        var product = new Core.Models.Inventory.Product
        {
            Name = "Test Feed",
            Manufacturer = "TestCo",
            Stock = 100m,
            UnitOfMeasure = UnitOfMeasure.Kilogram
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var consumption = new NewProductConsumptionDto(
            ProductId: product.Id,
            Stock: stock,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterProductConsumptionCommand.Args>(new(batch.Id, consumption));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "stock" && e.error.Contains("greater than zero"));
    }

    [Fact]
    public async Task RegisterProductConsumptionCommand_ShouldReturnValidationError_WhenInsufficientStock()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Insufficient Stock",
            Breed = "Layer",
            StartDate = DateTime.UtcNow,
            MaleCount = 50,
            FemaleCount = 50,
            UnsexedCount = 0,
            InitialPopulation = 100,
            Status = BatchStatus.Active,
            Shed = "Shed B-1"
        };
        dbContext.Batches.Add(batch);

        var product = new Core.Models.Inventory.Product
        {
            Name = "Limited Feed",
            Manufacturer = "FeedCo",
            Stock = 20m, // Only 20 kg available
            UnitOfMeasure = UnitOfMeasure.Kilogram
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var consumption = new NewProductConsumptionDto(
            ProductId: product.Id,
            Stock: 50m, // Trying to consume 50 kg
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterProductConsumptionCommand.Args>(new(batch.Id, consumption));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "stock" && e.error.Contains("Only 20"));
    }

    [Fact]
    public async Task RegisterProductConsumptionCommand_ShouldReturnValidationError_WhenInsufficientStockAfterConversion()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Conversion Insufficient",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = BatchStatus.Active,
            Shed = "Shed C-2"
        };
        dbContext.Batches.Add(batch);

        var product = new Core.Models.Inventory.Product
        {
            Name = "Supplement",
            Manufacturer = "BioVet",
            Stock = 5m, // 5 kg available
            UnitOfMeasure = UnitOfMeasure.Kilogram
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var consumption = new NewProductConsumptionDto(
            ProductId: product.Id,
            Stock: 8000m, // 8000 grams = 8 kg (more than available)
            UnitOfMeasure: "Gram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterProductConsumptionCommand.Args>(new(batch.Id, consumption));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "stock" && e.error.Contains("Only 5"));
    }

    [Fact]
    public async Task RegisterProductConsumptionCommand_ShouldReturnValidationError_ForInvalidUnitOfMeasure()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Invalid Unit",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = BatchStatus.Active,
            Shed = "Shed A-1"
        };
        dbContext.Batches.Add(batch);

        var product = new Core.Models.Inventory.Product
        {
            Name = "Test Feed",
            Manufacturer = "TestCo",
            Stock = 100m,
            UnitOfMeasure = UnitOfMeasure.Kilogram
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var consumption = new NewProductConsumptionDto(
            ProductId: product.Id,
            Stock: 10m,
            UnitOfMeasure: "InvalidUnit",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterProductConsumptionCommand.Args>(new(batch.Id, consumption));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "unitOfMeasure" && e.error.Contains("Invalid unit of measure"));
    }

    [Fact]
    public async Task RegisterProductConsumptionCommand_ShouldReturnValidationError_ForIncompatibleUnits()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Incompatible Units",
            Breed = "Layer",
            StartDate = DateTime.UtcNow,
            MaleCount = 50,
            FemaleCount = 50,
            UnsexedCount = 0,
            InitialPopulation = 100,
            Status = BatchStatus.Active,
            Shed = "Shed B-1"
        };
        dbContext.Batches.Add(batch);

        var product = new Core.Models.Inventory.Product
        {
            Name = "Liquid Disinfectant",
            Manufacturer = "CleanCo",
            Stock = 50m,
            UnitOfMeasure = UnitOfMeasure.Liter // Liquid measure
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var consumption = new NewProductConsumptionDto(
            ProductId: product.Id,
            Stock: 10m,
            UnitOfMeasure: "Kilogram", // Trying to use weight for liquid
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterProductConsumptionCommand.Args>(new(batch.Id, consumption));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "unitOfMeasure" && e.error.Contains("Cannot convert"));
    }

    [Fact]
    public async Task RegisterProductConsumptionCommand_ShouldReturnValidationError_ForInvalidDate()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Invalid Date",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = BatchStatus.Active,
            Shed = "Shed A-1"
        };
        dbContext.Batches.Add(batch);

        var product = new Core.Models.Inventory.Product
        {
            Name = "Test Feed",
            Manufacturer = "TestCo",
            Stock = 100m,
            UnitOfMeasure = UnitOfMeasure.Kilogram
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var consumption = new NewProductConsumptionDto(
            ProductId: product.Id,
            Stock: 10m,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: "invalid-date",
            Notes: null
        );
        var request = new AppRequest<RegisterProductConsumptionCommand.Args>(new(batch.Id, consumption));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "dateClientIsoString");
    }

    [Fact]
    public async Task RegisterProductConsumptionCommand_ShouldReturnValidationError_ForNotesTooLong()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Long Notes",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 100,
            FemaleCount = 100,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = BatchStatus.Active,
            Shed = "Shed A-1"
        };
        dbContext.Batches.Add(batch);

        var product = new Core.Models.Inventory.Product
        {
            Name = "Test Feed",
            Manufacturer = "TestCo",
            Stock = 100m,
            UnitOfMeasure = UnitOfMeasure.Kilogram
        };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var longNotes = new string('A', 501); // 501 characters
        var consumption = new NewProductConsumptionDto(
            ProductId: product.Id,
            Stock: 10m,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: longNotes
        );
        var request = new AppRequest<RegisterProductConsumptionCommand.Args>(new(batch.Id, consumption));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "notes" && e.error.Contains("500 characters"));
    }
}

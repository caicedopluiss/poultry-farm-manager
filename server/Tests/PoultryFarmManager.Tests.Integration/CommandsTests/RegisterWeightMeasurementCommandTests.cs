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

public class RegisterWeightMeasurementCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<RegisterWeightMeasurementCommand.Args, RegisterWeightMeasurementCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<RegisterWeightMeasurementCommand.Args, RegisterWeightMeasurementCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RegisterWeightMeasurementCommand_ShouldRegisterWeightMeasurement_WithValidData()
    {
        // Arrange - Create a batch
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Weight Measurement",
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

        var weightMeasurement = new NewWeightMeasurementDto(
            AverageWeight: 2.5m, // 2.5 kg
            SampleSize: 50,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Week 3 weight check"
        );
        var request = new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, weightMeasurement));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        var weightRecord = await dbContext.WeightMeasurementActivities.FindAsync(result.Value!.WeightMeasurement.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.WeightMeasurement.Id);
        Assert.Equal(batch.Id, result.Value!.WeightMeasurement.BatchId);
        Assert.Equal(2.5m, result.Value!.WeightMeasurement.AverageWeight);
        Assert.Equal(50, result.Value!.WeightMeasurement.SampleSize);
        Assert.Equal("Kilogram", result.Value!.WeightMeasurement.UnitOfMeasure);
        Assert.Equal("Week 3 weight check", result.Value!.WeightMeasurement.Notes);

        // Verify database record
        Assert.NotNull(weightRecord);
        Assert.Equal(batch.Id, weightRecord!.BatchId);
        Assert.Equal(2.5m, weightRecord.AverageWeight);
        Assert.Equal(50, weightRecord.SampleSize);
        Assert.Equal(UnitOfMeasure.Kilogram, weightRecord.UnitOfMeasure);
    }

    [Fact]
    public async Task RegisterWeightMeasurementCommand_ShouldRegisterWeightMeasurement_InGrams()
    {
        // Arrange - Create a batch
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Weight in Grams",
            Breed = "Layer",
            StartDate = DateTime.UtcNow,
            MaleCount = 80,
            FemaleCount = 120,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = BatchStatus.Active,
            Shed = "Shed B-1"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto(
            AverageWeight: 1850m, // 1850 grams
            SampleSize: 30,
            UnitOfMeasure: "Gram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Young layer weight"
        );
        var request = new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, weightMeasurement));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(1850m, result.Value!.WeightMeasurement.AverageWeight);
        Assert.Equal("Gram", result.Value!.WeightMeasurement.UnitOfMeasure);
        Assert.Equal(30, result.Value!.WeightMeasurement.SampleSize);
    }

    [Fact]
    public async Task RegisterWeightMeasurementCommand_ShouldAllowMultipleMeasurements_ForSameBatch()
    {
        // Arrange - Create a batch
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Multiple Measurements",
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
        await dbContext.SaveChangesAsync();

        // First measurement (Week 1)
        var measurement1 = new NewWeightMeasurementDto(
            AverageWeight: 0.5m,
            SampleSize: 40,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.AddDays(-14).ToString(Constants.DateTimeFormat),
            Notes: "Week 1"
        );
        var result1 = await handler.HandleAsync(new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, measurement1)), CancellationToken.None);

        // Second measurement (Week 2)
        var measurement2 = new NewWeightMeasurementDto(
            AverageWeight: 1.2m,
            SampleSize: 40,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.AddDays(-7).ToString(Constants.DateTimeFormat),
            Notes: "Week 2"
        );
        var result2 = await handler.HandleAsync(new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, measurement2)), CancellationToken.None);

        // Third measurement (Week 3)
        var measurement3 = new NewWeightMeasurementDto(
            AverageWeight: 2.1m,
            SampleSize: 40,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Week 3"
        );
        var result3 = await handler.HandleAsync(new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, measurement3)), CancellationToken.None);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.True(result3.IsSuccess);

        Assert.Equal(0.5m, result1.Value!.WeightMeasurement.AverageWeight);
        Assert.Equal(1.2m, result2.Value!.WeightMeasurement.AverageWeight);
        Assert.Equal(2.1m, result3.Value!.WeightMeasurement.AverageWeight);

        // Verify all three are different records
        Assert.NotEqual(result1.Value!.WeightMeasurement.Id, result2.Value!.WeightMeasurement.Id);
        Assert.NotEqual(result2.Value!.WeightMeasurement.Id, result3.Value!.WeightMeasurement.Id);
        Assert.NotEqual(result1.Value!.WeightMeasurement.Id, result3.Value!.WeightMeasurement.Id);
    }

    [Fact]
    public async Task RegisterWeightMeasurementCommand_ShouldThrowException_WhenBatchDoesNotExist()
    {
        // Arrange
        var nonExistentBatchId = Guid.NewGuid();
        var weightMeasurement = new NewWeightMeasurementDto(
            AverageWeight: 2.0m,
            SampleSize: 20,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterWeightMeasurementCommand.Args>(new(nonExistentBatchId, weightMeasurement));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(request, CancellationToken.None)
        );

        Assert.Contains("not found", exception.Message);
        Assert.Contains(nonExistentBatchId.ToString(), exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-2.5)]
    public async Task RegisterWeightMeasurementCommand_ShouldReturnValidationError_ForInvalidAverageWeight(decimal weight)
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Invalid Weight",
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

        var weightMeasurement = new NewWeightMeasurementDto(
            AverageWeight: weight,
            SampleSize: 20,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, weightMeasurement));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "averageWeight" && e.error.Contains("greater than zero"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task RegisterWeightMeasurementCommand_ShouldReturnValidationError_ForInvalidSampleSize(int sampleSize)
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Invalid Sample Size",
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

        var weightMeasurement = new NewWeightMeasurementDto(
            AverageWeight: 2.5m,
            SampleSize: sampleSize,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, weightMeasurement));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "sampleSize" && e.error.Contains("greater than zero"));
    }

    [Theory]
    [InlineData("InvalidUnit")]
    [InlineData("Pounds")]
    [InlineData("")]
    public async Task RegisterWeightMeasurementCommand_ShouldReturnValidationError_ForInvalidUnitOfMeasure(string unit)
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
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto(
            AverageWeight: 2.5m,
            SampleSize: 20,
            UnitOfMeasure: unit,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, weightMeasurement));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "unitOfMeasure" && (e.error.Contains("Unit of measure is required") || e.error.Contains("Invalid unit of measure")));
    }

    [Fact]
    public async Task RegisterWeightMeasurementCommand_ShouldReturnValidationError_ForInvalidDate()
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
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto(
            AverageWeight: 2.5m,
            SampleSize: 20,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: "invalid-date-format",
            Notes: null
        );
        var request = new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, weightMeasurement));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "dateClientIsoString" && e.error.Contains("valid ISO 8601"));
    }

    [Fact]
    public async Task RegisterWeightMeasurementCommand_ShouldReturnValidationError_WhenNotesExceedMaxLength()
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
        await dbContext.SaveChangesAsync();

        var longNotes = new string('A', 501); // 501 characters
        var weightMeasurement = new NewWeightMeasurementDto(
            AverageWeight: 2.5m,
            SampleSize: 20,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: longNotes
        );
        var request = new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, weightMeasurement));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "notes" && e.error.Contains("500 characters"));
    }

    [Fact]
    public async Task RegisterWeightMeasurementCommand_ShouldAcceptNullNotes()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Null Notes",
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

        var weightMeasurement = new NewWeightMeasurementDto(
            AverageWeight: 2.5m,
            SampleSize: 20,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, weightMeasurement));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.WeightMeasurement.Notes);
    }

    [Fact]
    public async Task RegisterWeightMeasurementCommand_ShouldFail_WhenBatchIsProcessed()
    {
        // Arrange - Create a batch with Processed status
        var batch = new Core.Models.Batch
        {
            Name = "Processed Batch",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow.AddDays(-60),
            MaleCount = 0,
            FemaleCount = 0,
            UnsexedCount = 0,
            InitialPopulation = 200,
            Status = BatchStatus.Processed,
            Shed = "Shed A-1"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto(
            AverageWeight: 2.5m,
            SampleSize: 20,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: "Attempting weight measurement on processed batch"
        );
        var request = new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, weightMeasurement));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchId" && e.error.Contains("Only Active batches"));
    }

    [Fact]
    public async Task RegisterWeightMeasurementCommand_ShouldFail_WhenBatchIsCanceled()
    {
        // Arrange - Create a batch with Canceled status
        var batch = new Core.Models.Batch
        {
            Name = "Canceled Batch",
            Breed = "Layer",
            StartDate = DateTime.UtcNow.AddDays(-30),
            MaleCount = 50,
            FemaleCount = 50,
            UnsexedCount = 0,
            InitialPopulation = 100,
            Status = BatchStatus.Canceled,
            Shed = "Shed B-2"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var weightMeasurement = new NewWeightMeasurementDto(
            AverageWeight: 1.8m,
            SampleSize: 15,
            UnitOfMeasure: "Kilogram",
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, weightMeasurement));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "batchId" && e.error.Contains("Only Active batches"));
    }

    [Fact]
    public async Task RegisterWeightMeasurementCommand_ShouldFail_WhenUnitOfMeasureIsNotWeightUnit()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Invalid Unit",
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

        var weightMeasurement = new NewWeightMeasurementDto(
            AverageWeight: 2.5m,
            SampleSize: 20,
            UnitOfMeasure: "Liter", // Invalid: not a weight unit
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, weightMeasurement));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "unitOfMeasure" && e.error.Contains("Only weight units"));
    }

    [Fact]
    public async Task RegisterWeightMeasurementCommand_ShouldFail_WhenUnitOfMeasureIsUnit()
    {
        // Arrange
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Unit",
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

        var weightMeasurement = new NewWeightMeasurementDto(
            AverageWeight: 2.5m,
            SampleSize: 20,
            UnitOfMeasure: "Unit", // Invalid: not a weight unit
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Notes: null
        );
        var request = new AppRequest<RegisterWeightMeasurementCommand.Args>(new(batch.Id, weightMeasurement));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "unitOfMeasure" && e.error.Contains("Only weight units"));
    }
}

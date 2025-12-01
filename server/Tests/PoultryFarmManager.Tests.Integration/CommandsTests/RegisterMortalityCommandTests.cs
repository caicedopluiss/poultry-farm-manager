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

public class RegisterMortalityCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<RegisterMortalityCommand.Args, RegisterMortalityCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<RegisterMortalityCommand.Args, RegisterMortalityCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RegisterMortalityCommand_ShouldRegisterMortality_AndUpdateBatchPopulation()
    {
        // Arrange - Create a batch directly in DB
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch for Mortality",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 50,
            FemaleCount = 50,
            UnsexedCount = 20,
            InitialPopulation = 120,
            Status = BatchStatus.Active,
            Shed = "Shed A-1"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        var newMortality = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 10,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Unsexed",
            Notes: "Disease outbreak"
        );
        var request = new AppRequest<RegisterMortalityCommand.Args>(new(batchId, newMortality));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Detach the batch entity so we can reload it from the database
        dbContext.Entry(batch).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        var updatedBatch = await dbContext.Batches.FindAsync(batchId);
        var mortalityRecord = await dbContext.MortalityRegistrationActivities.FindAsync(result.Value!.MortalityRegistration.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.MortalityRegistration.Id);
        Assert.Equal(batchId, result.Value!.MortalityRegistration.BatchId);
        Assert.Equal(10, result.Value!.MortalityRegistration.NumberOfDeaths);
        Assert.Equal("Disease outbreak", result.Value!.MortalityRegistration.Notes);

        // Verify batch population was updated (unsexed reduced first)
        Assert.NotNull(updatedBatch);
        Assert.Equal(110, updatedBatch!.Population); // 120 - 10 = 110
        Assert.Equal(10, updatedBatch.UnsexedCount); // 20 - 10 = 10
        Assert.Equal(50, updatedBatch.MaleCount); // Unchanged
        Assert.Equal(50, updatedBatch.FemaleCount); // Unchanged

        Assert.NotNull(mortalityRecord);
    }

    [Fact]
    public async Task RegisterMortalityCommand_WithMultipleMortalityRegistrations_ShouldReduceCorrectly()
    {
        // Arrange - Create a batch directly in DB
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Mixed Reduction",
            Breed = "Layer",
            StartDate = DateTime.UtcNow,
            MaleCount = 40,
            FemaleCount = 60,
            UnsexedCount = 5,
            InitialPopulation = 105,
            Status = BatchStatus.Active,
            Shed = "Shed B-2"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        // Test unsexed reduction first
        var mortalityUnsexed = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 5,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Unsexed",
            Notes: null
        );
        await handler.HandleAsync(new AppRequest<RegisterMortalityCommand.Args>(new(batchId, mortalityUnsexed)), CancellationToken.None);

        // Then test male reduction
        var mortalityMale = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 10,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Male",
            Notes: null
        );
        var result = await handler.HandleAsync(new AppRequest<RegisterMortalityCommand.Args>(new(batchId, mortalityMale)), CancellationToken.None);

        // Detach the batch entity so we can reload it from the database
        dbContext.Entry(batch).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        var updatedBatch = await dbContext.Batches.FindAsync(batchId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(updatedBatch);
        Assert.Equal(90, updatedBatch!.Population); // 105 - 5 - 10 = 90
        Assert.Equal(0, updatedBatch.UnsexedCount); // All 5 unsexed removed
        Assert.Equal(30, updatedBatch.MaleCount); // 40 - 10 = 30
        Assert.Equal(60, updatedBatch.FemaleCount); // Unchanged
    }

    [Fact]
    public async Task RegisterMortalityCommand_ShouldThrowInvalidOperationException_WhenBatchDoesNotExist()
    {
        // Arrange
        var nonExistentBatchId = Guid.NewGuid();
        var newMortality = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 5,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Unsexed",
            Notes: null
        );
        var request = new AppRequest<RegisterMortalityCommand.Args>(new(nonExistentBatchId, newMortality));

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
    [InlineData(-10)]
    public async Task RegisterMortalityCommand_ShouldReturnValidationError_ForInvalidNumberOfDeaths(int numberOfDeaths)
    {
        // Arrange - Create a batch directly in DB for test setup
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch For Invalid Deaths",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 50,
            FemaleCount = 50,
            UnsexedCount = 20,
            InitialPopulation = 120,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed T-1"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var newMortality = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: numberOfDeaths,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Unsexed",
            Notes: null
        );
        var request = new AppRequest<RegisterMortalityCommand.Args>(new(batch.Id, newMortality));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "numberOfDeaths");
    }

    [Fact]
    public async Task RegisterMortalityCommand_ShouldReturnValidationError_ForInvalidDate()
    {
        // Arrange - Create a batch directly in DB for test setup
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch For Invalid Date",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 50,
            FemaleCount = 50,
            UnsexedCount = 20,
            InitialPopulation = 120,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed T-2"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var newMortality = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 5,
            DateClientIsoString: "invalid-date",
            Sex: "Unsexed",
            Notes: null
        );
        var request = new AppRequest<RegisterMortalityCommand.Args>(new(batch.Id, newMortality));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "dateClientIsoString");
    }

    [Fact]
    public async Task RegisterMortalityCommand_ShouldReturnValidationError_ForNotesExceedingMaxLength()
    {
        // Arrange - Create a batch directly in DB for test setup
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch For Invalid Notes",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 50,
            FemaleCount = 50,
            UnsexedCount = 20,
            InitialPopulation = 120,
            Status = Core.Enums.BatchStatus.Active,
            Shed = "Shed T-3"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();

        var newMortality = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 5,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Unsexed",
            Notes: new string('A', 501) // Exceeds 500 char limit
        );
        var request = new AppRequest<RegisterMortalityCommand.Args>(new(batch.Id, newMortality));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "notes");
    }

    [Fact]
    public async Task RegisterMortalityCommand_WithMaleSex_ShouldReduceMaleCountOnly()
    {
        // Arrange - Create a batch directly in DB
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Male Mortality",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 50,
            FemaleCount = 50,
            UnsexedCount = 20,
            InitialPopulation = 120,
            Status = BatchStatus.Active,
            Shed = "Shed C-3"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        var newMortality = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 15,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Male",
            Notes: "Male-specific disease"
        );
        var request = new AppRequest<RegisterMortalityCommand.Args>(new(batchId, newMortality));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Detach the batch entity so we can reload it from the database
        dbContext.Entry(batch).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        var updatedBatch = await dbContext.Batches.FindAsync(batchId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(updatedBatch);
        Assert.Equal(105, updatedBatch!.Population); // 120 - 15 = 105
        Assert.Equal(35, updatedBatch.MaleCount); // 50 - 15 = 35
        Assert.Equal(50, updatedBatch.FemaleCount); // Unchanged
        Assert.Equal(20, updatedBatch.UnsexedCount); // Unchanged
        Assert.Equal("Male", result.Value!.MortalityRegistration.Sex);
    }

    [Fact]
    public async Task RegisterMortalityCommand_WithFemaleSex_ShouldReduceFemaleCountOnly()
    {
        // Arrange - Create a batch directly in DB
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Female Mortality",
            Breed = "Layer",
            StartDate = DateTime.UtcNow,
            MaleCount = 30,
            FemaleCount = 70,
            UnsexedCount = 10,
            InitialPopulation = 110,
            Status = BatchStatus.Active,
            Shed = "Shed D-4"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        var newMortality = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 20,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Female",
            Notes: null
        );
        var request = new AppRequest<RegisterMortalityCommand.Args>(new(batchId, newMortality));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Detach the batch entity so we can reload it from the database
        dbContext.Entry(batch).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        var updatedBatch = await dbContext.Batches.FindAsync(batchId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(updatedBatch);
        Assert.Equal(90, updatedBatch!.Population); // 110 - 20 = 90
        Assert.Equal(30, updatedBatch.MaleCount); // Unchanged
        Assert.Equal(50, updatedBatch.FemaleCount); // 70 - 20 = 50
        Assert.Equal(10, updatedBatch.UnsexedCount); // Unchanged
        Assert.Equal("Female", result.Value!.MortalityRegistration.Sex);
    }

    [Fact]
    public async Task RegisterMortalityCommand_WithUnsexedSex_ShouldReduceUnsexedCountOnly()
    {
        // Arrange - Create a batch directly in DB
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Unsexed Mortality",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 40,
            FemaleCount = 40,
            UnsexedCount = 30,
            InitialPopulation = 110,
            Status = BatchStatus.Active,
            Shed = "Shed E-5"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        var newMortality = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 10,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Unsexed",
            Notes: "Early chick mortality"
        );
        var request = new AppRequest<RegisterMortalityCommand.Args>(new(batchId, newMortality));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Detach the batch entity so we can reload it from the database
        dbContext.Entry(batch).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        var updatedBatch = await dbContext.Batches.FindAsync(batchId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(updatedBatch);
        Assert.Equal(100, updatedBatch!.Population); // 110 - 10 = 100
        Assert.Equal(40, updatedBatch.MaleCount); // Unchanged
        Assert.Equal(40, updatedBatch.FemaleCount); // Unchanged
        Assert.Equal(20, updatedBatch.UnsexedCount); // 30 - 10 = 20
        Assert.Equal("Unsexed", result.Value!.MortalityRegistration.Sex);
    }

    [Fact]
    public async Task RegisterMortalityCommand_ShouldReturnValidationError_WhenMaleDeathsExceedAvailable()
    {
        // Arrange - Create a batch directly in DB with limited males
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Limited Males",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 10,
            FemaleCount = 50,
            UnsexedCount = 40,
            InitialPopulation = 100,
            Status = BatchStatus.Active,
            Shed = "Shed F-6"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        var newMortality = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 15, // More than available males
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Male",
            Notes: null
        );
        var request = new AppRequest<RegisterMortalityCommand.Args>(new(batchId, newMortality));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "numberOfDeaths" && e.error.Contains("male"));
    }

    [Fact]
    public async Task RegisterMortalityCommand_ShouldReturnValidationError_WhenFemaleDeathsExceedAvailable()
    {
        // Arrange - Create a batch directly in DB with limited females
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Limited Females",
            Breed = "Layer",
            StartDate = DateTime.UtcNow,
            MaleCount = 50,
            FemaleCount = 5,
            UnsexedCount = 20,
            InitialPopulation = 75,
            Status = BatchStatus.Active,
            Shed = "Shed G-7"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        var newMortality = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 10, // More than available females
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Female",
            Notes: null
        );
        var request = new AppRequest<RegisterMortalityCommand.Args>(new(batchId, newMortality));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "numberOfDeaths" && e.error.Contains("female"));
    }

    [Fact]
    public async Task RegisterMortalityCommand_ShouldReturnValidationError_WhenUnsexedDeathsExceedAvailable()
    {
        // Arrange - Create a batch directly in DB with limited unsexed
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Limited Unsexed",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 40,
            FemaleCount = 40,
            UnsexedCount = 3,
            InitialPopulation = 83,
            Status = BatchStatus.Active,
            Shed = "Shed H-8"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        var newMortality = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 10, // More than available unsexed
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "Unsexed",
            Notes: null
        );
        var request = new AppRequest<RegisterMortalityCommand.Args>(new(batchId, newMortality));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "numberOfDeaths" && e.error.Contains("unsexed"));
    }

    [Fact]
    public async Task RegisterMortalityCommand_ShouldReturnValidationError_ForInvalidSexValue()
    {
        // Arrange - Create a batch directly in DB
        var batch = new Core.Models.Batch
        {
            Name = "Test Batch Invalid Sex",
            Breed = "Broiler",
            StartDate = DateTime.UtcNow,
            MaleCount = 40,
            FemaleCount = 40,
            UnsexedCount = 20,
            InitialPopulation = 100,
            Status = BatchStatus.Active,
            Shed = "Shed I-9"
        };
        dbContext.Batches.Add(batch);
        await dbContext.SaveChangesAsync();
        var batchId = batch.Id;

        var newMortality = new NewMortalityRegistrationDto
        (
            NumberOfDeaths: 10,
            DateClientIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            Sex: "InvalidSex", // Invalid sex value
            Notes: null
        );
        var request = new AppRequest<RegisterMortalityCommand.Args>(new(batchId, newMortality));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "sex" && e.error.Contains("Invalid sex value"));
    }
}

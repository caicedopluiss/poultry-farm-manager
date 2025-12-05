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

public class CreateBatchCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<CreateBatchCommand.Args, CreateBatchCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<CreateBatchCommand.Args, CreateBatchCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateBatchCommand_ShouldCreateBatch()
    {
        // Arrange
        var newBatch = new NewBatchDto
        (
            Name: "Test Batch",
            Breed: null,
            StartClientDateIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            MaleCount: 50,
            FemaleCount: 50,
            UnsexedCount: 0,
            Shed: "Shed A-1"
        );
        var request = new AppRequest<CreateBatchCommand.Args>(new(newBatch));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        var createdBatch = await dbContext.Batches.FindAsync(result.Value!.CreatedBatch.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.CreatedBatch.Id);
        Assert.Equal(100, result.Value!.CreatedBatch.InitialPopulation);
        Assert.Equal(nameof(BatchStatus.Active), result.Value!.CreatedBatch.Status);
        Assert.NotNull(createdBatch);
        Assert.Equal(createdBatch.Id, result.Value!.CreatedBatch.Id);
    }

    // future date -> Planned status
    [Fact]
    public async Task CreateBatchCommand_ShouldCreatePlannedBatch_ForFutureStartDate()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(10).ToString(Constants.DateTimeFormat);
        var newBatch = new NewBatchDto
        (
            Name: "Future Batch",
            Breed: "Leghorn",
            StartClientDateIsoString: futureDate,
            MaleCount: 20,
            FemaleCount: 30,
            UnsexedCount: 0,
            Shed: "Shed B-2"
        );
        var request = new AppRequest<CreateBatchCommand.Args>(new(newBatch));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(nameof(BatchStatus.Active), result.Value!.CreatedBatch.Status);
    }

    [Theory]
    [InlineData(new[] { -1, 0, 0 })]
    [InlineData(new[] { 0, -1, 0 })]
    [InlineData(new[] { 0, 0, -1 })]
    [InlineData(new[] { 0, 0, 0 })]
    public async Task CreateBatchCommand_ShouldReturnValidationErrors_ForInvalidInput(int[] counts)
    {
        // Arrange
        var newBatch = new NewBatchDto
        (
            Name: "", // Invalid: Name is required
            Breed: new string('B', 101), // Invalid: Breed exceeds max length
            StartClientDateIsoString: "invalid-date", // Invalid date format
            MaleCount: counts[0],
            FemaleCount: counts[1],
            UnsexedCount: counts[2],
            Shed: null
        );
        var request = new AppRequest<CreateBatchCommand.Args>(new(newBatch));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
        Assert.Null(result.Value);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
        Assert.Contains(result.ValidationErrors, e => e.field == "breed");
        Assert.Contains(result.ValidationErrors, e => e.field == "startClientDateIsoString");
        Assert.Contains(result.ValidationErrors, e => e.field == "maleCount" || e.field == "femaleCount" || e.field == "unsexedCount" || e.field == "population");
    }

    [Fact]
    public async Task CreateBatchCommand_ShouldReturnValidationError_ForNameExceedingMaxLength()
    {
        // Arrange
        var newBatch = new NewBatchDto
        (
            Name: new string('A', 101), // Invalid: Name exceeds max length
            Breed: "Test Breed",
            StartClientDateIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            MaleCount: 10,
            FemaleCount: 10,
            UnsexedCount: 0,
            Shed: "Test Shed"
        );
        var request = new AppRequest<CreateBatchCommand.Args>(new(newBatch));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
        Assert.Null(result.Value);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateBatchCommand_ShouldReturnValidationError_ForShedExceedingMaxLength()
    {
        // Arrange
        var newBatch = new NewBatchDto
        (
            Name: "Valid Batch Name",
            Breed: "Test Breed",
            StartClientDateIsoString: DateTime.UtcNow.ToString(Constants.DateTimeFormat),
            MaleCount: 10,
            FemaleCount: 10,
            UnsexedCount: 0,
            Shed: new string('S', 101) // Invalid: Shed exceeds max length
        );
        var request = new AppRequest<CreateBatchCommand.Args>(new(newBatch));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
        Assert.Null(result.Value);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "shed");
        Assert.Single(result.ValidationErrors);
    }
}

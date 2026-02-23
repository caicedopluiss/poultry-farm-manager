using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Commands.Assets;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class CreateAssetCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<CreateAssetCommand.Args, CreateAssetCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<CreateAssetCommand.Args, CreateAssetCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateAssetCommand_ShouldCreateAsset_WithValidData()
    {
        // Arrange
        var vendor = await dbContext.CreateVendorAsync();
        var newAssetDto = new NewAssetDto(
            Name: "Test Incubator",
            Description: "Automatic incubator",
            InitialQuantity: 5,
            Notes: "Testing notes",
            VendorId: vendor.Id,
            UnitPrice: 100m
        );
        var request = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.CreatedAsset.Id);
        Assert.Equal("Test Incubator", result.Value!.CreatedAsset.Name);
        Assert.Equal("Automatic incubator", result.Value!.CreatedAsset.Description);
        Assert.NotNull(result.Value!.CreatedAsset.States);
        Assert.Single(result.Value!.CreatedAsset.States);
        Assert.Equal(5, result.Value!.CreatedAsset.States.First().Quantity);
        Assert.Equal(nameof(AssetStatus.Available), result.Value!.CreatedAsset.States.First().Status);

        var assetInDb = await dbContext.Assets.FindAsync(result.Value!.CreatedAsset.Id);
        Assert.NotNull(assetInDb);
        Assert.Equal("Test Incubator", assetInDb.Name);
    }

    [Fact]
    public async Task CreateAssetCommand_ShouldFail_WithEmptyName()
    {
        // Arrange
        var vendor = await dbContext.CreateVendorAsync();
        var newAssetDto = new NewAssetDto(
            Name: "",
            Description: "Test",
            InitialQuantity: 1,
            Notes: null,
            VendorId: vendor.Id,
            UnitPrice: 100m
        );
        var request = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateAssetCommand_ShouldFail_WithNameExceedingMaxLength()
    {
        // Arrange
        var vendor = await dbContext.CreateVendorAsync();
        var newAssetDto = new NewAssetDto(
            Name: new string('A', 101), // 101 characters - exceeds max length of 100
            Description: "Valid description",
            InitialQuantity: 1,
            Notes: null,
            VendorId: vendor.Id,
            UnitPrice: 100m
        );
        var request = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));

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
    public async Task CreateAssetCommand_ShouldFail_WithDescriptionExceedingMaxLength()
    {
        // Arrange
        var vendor = await dbContext.CreateVendorAsync();
        var newAssetDto = new NewAssetDto(
            Name: "Valid Asset Name",
            Description: new string('B', 501), // 501 characters - exceeds max length of 500
            InitialQuantity: 1,
            Notes: null,
            VendorId: vendor.Id,
            UnitPrice: 100m
        );
        var request = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));

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
    public async Task CreateAssetCommand_ShouldFail_WithNotesExceedingMaxLength()
    {
        // Arrange
        var vendor = await dbContext.CreateVendorAsync();
        var newAssetDto = new NewAssetDto(
            Name: "Valid Asset Name",
            Description: "Valid description",
            InitialQuantity: 1,
            Notes: new string('D', 501), // 501 characters - exceeds max length of 500
            VendorId: vendor.Id,
            UnitPrice: 100m
        );
        var request = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "notes");
        Assert.Single(result.ValidationErrors);
    }

    [Fact]
    public async Task CreateAssetCommand_ShouldFail_WithMultipleValidationErrors()
    {
        // Arrange
        var vendor = await dbContext.CreateVendorAsync();
        var newAssetDto = new NewAssetDto(
            Name: "", // Empty name
            Description: new string('B', 501), // Description too long
            InitialQuantity: 0, // Invalid quantity (must be > 0)
            Notes: new string('D', 501), // Notes too long
            VendorId: vendor.Id,
            UnitPrice: 100m
        );
        var request = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "name");
        Assert.Contains(result.ValidationErrors, e => e.field == "description");
        Assert.Contains(result.ValidationErrors, e => e.field == "initialQuantity");
        Assert.Contains(result.ValidationErrors, e => e.field == "notes");
        Assert.Equal(4, result.ValidationErrors.Count());
    }

    [Fact]
    public async Task CreateAssetCommand_ShouldCreateAsset_WithoutVendor()
    {
        // Arrange - Asset from a gift, no vendor or price
        var newAssetDto = new NewAssetDto(
            Name: "Gift Incubator",
            Description: "Received as gift",
            InitialQuantity: 2,
            Notes: "Gift from partner farm",
            VendorId: null,
            UnitPrice: null
        );
        var request = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.CreatedAsset.Id);
        Assert.Equal("Gift Incubator", result.Value!.CreatedAsset.Name);
        Assert.Equal(2, result.Value!.CreatedAsset.States.First().Quantity);

        // Verify no transaction was created
        var transactions = dbContext.Transactions.Where(t => t.AssetId == result.Value!.CreatedAsset.Id).ToList();
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task CreateAssetCommand_ShouldFail_WithVendorButNoUnitPrice()
    {
        // Arrange
        var vendor = await dbContext.CreateVendorAsync();
        var newAssetDto = new NewAssetDto(
            Name: "Test Asset",
            Description: "Test",
            InitialQuantity: 1,
            Notes: null,
            VendorId: vendor.Id,
            UnitPrice: null // Missing unit price when vendor is provided
        );
        var request = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "unitPrice" && e.error.Contains("required when vendor"));
    }

    [Fact]
    public async Task CreateAssetCommand_ShouldFail_WithUnitPriceButNoVendor()
    {
        // Arrange
        var newAssetDto = new NewAssetDto(
            Name: "Test Asset",
            Description: "Test",
            InitialQuantity: 1,
            Notes: null,
            VendorId: null,
            UnitPrice: 100m // Unit price without vendor
        );
        var request = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "vendorId" && e.error.Contains("required when unit price"));
    }

    [Fact]
    public async Task CreateAssetCommand_ShouldFail_WithInvalidVendorId()
    {
        // Arrange
        var nonExistentVendorId = Guid.NewGuid();
        var newAssetDto = new NewAssetDto(
            Name: "Test Asset",
            Description: "Test",
            InitialQuantity: 1,
            Notes: null,
            VendorId: nonExistentVendorId,
            UnitPrice: 100m
        );
        var request = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "vendorId" && e.error == "Vendor not found.");
    }

    [Fact]
    public async Task CreateAssetCommand_ShouldFail_WithZeroUnitPrice()
    {
        // Arrange
        var vendor = await dbContext.CreateVendorAsync();
        var newAssetDto = new NewAssetDto(
            Name: "Test Asset",
            Description: "Test",
            InitialQuantity: 1,
            Notes: null,
            VendorId: vendor.Id,
            UnitPrice: 0m // Invalid: zero unit price
        );
        var request = new AppRequest<CreateAssetCommand.Args>(new(newAssetDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "unitPrice");
    }
}

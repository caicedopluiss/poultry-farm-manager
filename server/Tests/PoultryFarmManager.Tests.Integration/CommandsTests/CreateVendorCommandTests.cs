using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.Vendors;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models.Finance;
using Xunit;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class CreateVendorCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<CreateVendorCommand.Args, CreateVendorCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<CreateVendorCommand.Args, CreateVendorCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateVendorCommand_ShouldSucceed_WithValidData()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var newVendor = new NewVendorDto(
            Name: "ABC Suppliers",
            Location: "123 Main St",
            ContactPersonId: contactPerson.Id
        );
        var request = new AppRequest<CreateVendorCommand.Args>(new(newVendor));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.CreatedVendor.Id);
        Assert.Equal("ABC Suppliers", result.Value.CreatedVendor.Name);
        Assert.Equal("123 Main St", result.Value.CreatedVendor.Location);
        Assert.Equal(contactPerson.Id, result.Value.CreatedVendor.ContactPersonId);
        Assert.NotNull(result.Value.CreatedVendor.ContactPerson);
        Assert.Equal(contactPerson.Id, result.Value.CreatedVendor.ContactPerson.Id);
    }

    [Fact]
    public async Task CreateVendorCommand_ShouldSucceed_WithMinimumFields()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "Jane",
            LastName = "Smith"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var newVendor = new NewVendorDto(
            Name: "XYZ Company",
            Location: null,
            ContactPersonId: contactPerson.Id
        );
        var request = new AppRequest<CreateVendorCommand.Args>(new(newVendor));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("XYZ Company", result.Value.CreatedVendor.Name);
        Assert.Null(result.Value.CreatedVendor.Location);
        Assert.Equal(contactPerson.Id, result.Value.CreatedVendor.ContactPersonId);
    }

    [Fact]
    public async Task CreateVendorCommand_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "Test",
            LastName = "User"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var newVendor = new NewVendorDto(
            Name: "",
            Location: null,
            ContactPersonId: contactPerson.Id
        );
        var request = new AppRequest<CreateVendorCommand.Args>(new(newVendor));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "name" && e.error == "Vendor name is required.");
    }

    [Fact]
    public async Task CreateVendorCommand_ShouldFail_WhenNameIsWhitespace()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "Test",
            LastName = "User"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var newVendor = new NewVendorDto(
            Name: "   ",
            Location: null,
            ContactPersonId: contactPerson.Id
        );
        var request = new AppRequest<CreateVendorCommand.Args>(new(newVendor));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "name" && e.error == "Vendor name is required.");
    }

    [Fact]
    public async Task CreateVendorCommand_ShouldFail_WhenNameExceeds100Characters()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "Test",
            LastName = "User"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var longName = new string('A', 101);
        var newVendor = new NewVendorDto(
            Name: longName,
            Location: null,
            ContactPersonId: contactPerson.Id
        );
        var request = new AppRequest<CreateVendorCommand.Args>(new(newVendor));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "name" && e.error == "Vendor name cannot exceed 100 characters.");
    }

    [Fact]
    public async Task CreateVendorCommand_ShouldFail_WhenLocationExceeds100Characters()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "Test",
            LastName = "User"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var longLocation = new string('A', 101);
        var newVendor = new NewVendorDto(
            Name: "Valid Name",
            Location: longLocation,
            ContactPersonId: contactPerson.Id
        );
        var request = new AppRequest<CreateVendorCommand.Args>(new(newVendor));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "location" && e.error == "Location cannot exceed 100 characters.");
    }

    [Fact]
    public async Task CreateVendorCommand_ShouldFail_WhenContactPersonIdIsEmpty()
    {
        // Arrange
        var newVendor = new NewVendorDto(
            Name: "Valid Name",
            Location: null,
            ContactPersonId: Guid.Empty
        );
        var request = new AppRequest<CreateVendorCommand.Args>(new(newVendor));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "contactPersonId" && e.error == "Contact person is required.");
    }

    [Fact]
    public async Task CreateVendorCommand_ShouldFail_WhenContactPersonDoesNotExist()
    {
        // Arrange
        var nonExistentPersonId = Guid.NewGuid();
        var newVendor = new NewVendorDto(
            Name: "Valid Name",
            Location: null,
            ContactPersonId: nonExistentPersonId
        );
        var request = new AppRequest<CreateVendorCommand.Args>(new(newVendor));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "contactPersonId" && e.error == "Contact person does not exist.");
    }

    [Fact]
    public async Task CreateVendorCommand_ShouldFail_WithMultipleValidationErrors()
    {
        // Arrange
        var longName = new string('A', 101);
        var longLocation = new string('B', 101);
        var newVendor = new NewVendorDto(
            Name: longName,
            Location: longLocation,
            ContactPersonId: Guid.Empty
        );
        var request = new AppRequest<CreateVendorCommand.Args>(new(newVendor));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "name" && e.error == "Vendor name cannot exceed 100 characters.");
        Assert.Contains(result.ValidationErrors, e => e.field == "location" && e.error == "Location cannot exceed 100 characters.");
        Assert.Contains(result.ValidationErrors, e => e.field == "contactPersonId" && e.error == "Contact person is required.");
    }

    [Fact]
    public async Task CreateVendorCommand_ShouldSucceed_WithExactly100CharacterName()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "Test",
            LastName = "User"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var exactName = new string('A', 100);
        var newVendor = new NewVendorDto(
            Name: exactName,
            Location: null,
            ContactPersonId: contactPerson.Id
        );
        var request = new AppRequest<CreateVendorCommand.Args>(new(newVendor));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(exactName, result.Value!.CreatedVendor.Name);
    }

    [Fact]
    public async Task CreateVendorCommand_ShouldSucceed_WithExactly100CharacterLocation()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "Test",
            LastName = "User"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var exactLocation = new string('A', 100);
        var newVendor = new NewVendorDto(
            Name: "Valid Name",
            Location: exactLocation,
            ContactPersonId: contactPerson.Id
        );
        var request = new AppRequest<CreateVendorCommand.Args>(new(newVendor));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(exactLocation, result.Value!.CreatedVendor.Location);
    }
}

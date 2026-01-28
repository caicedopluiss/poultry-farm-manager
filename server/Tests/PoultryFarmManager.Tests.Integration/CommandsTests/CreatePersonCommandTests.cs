using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Commands.Persons;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class CreatePersonCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<CreatePersonCommand.Args, CreatePersonCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<CreatePersonCommand.Args, CreatePersonCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreatePersonCommand_ShouldCreatePerson_WithAllFields()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            PhoneNumber: "555-0100",
            Location: "Downtown Office"
        );
        var request = new AppRequest<CreatePersonCommand.Args>(new(newPerson));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        var createdPerson = await dbContext.Persons.FindAsync(result.Value!.CreatedPerson.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.CreatedPerson.Id);
        Assert.Equal("John", result.Value!.CreatedPerson.FirstName);
        Assert.Equal("Doe", result.Value!.CreatedPerson.LastName);
        Assert.Equal("john.doe@example.com", result.Value!.CreatedPerson.Email);
        Assert.Equal("555-0100", result.Value!.CreatedPerson.PhoneNumber);
        Assert.Equal("Downtown Office", result.Value!.CreatedPerson.Location);

        // Verify database record
        Assert.NotNull(createdPerson);
        Assert.Equal(createdPerson!.Id, result.Value!.CreatedPerson.Id);
        Assert.Equal("John", createdPerson.FirstName);
        Assert.Equal("Doe", createdPerson.LastName);
    }

    [Fact]
    public async Task CreatePersonCommand_ShouldCreatePerson_WithOnlyRequiredFields()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "Jane",
            LastName: "Smith",
            Email: null,
            PhoneNumber: null,
            Location: null
        );
        var request = new AppRequest<CreatePersonCommand.Args>(new(newPerson));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("Jane", result.Value!.CreatedPerson.FirstName);
        Assert.Equal("Smith", result.Value!.CreatedPerson.LastName);
        Assert.Null(result.Value!.CreatedPerson.Email);
        Assert.Null(result.Value!.CreatedPerson.PhoneNumber);
        Assert.Null(result.Value!.CreatedPerson.Location);
    }

    [Fact]
    public async Task CreatePersonCommand_ShouldFail_WhenFirstNameIsEmpty()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "",
            LastName: "Doe",
            Email: null,
            PhoneNumber: null,
            Location: null
        );
        var request = new AppRequest<CreatePersonCommand.Args>(new(newPerson));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(result.ValidationErrors, e => e.field == "firstName" && e.error == "First name is required.");
    }

    [Fact]
    public async Task CreatePersonCommand_ShouldFail_WhenFirstNameIsWhitespace()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "   ",
            LastName: "Doe",
            Email: null,
            PhoneNumber: null,
            Location: null
        );
        var request = new AppRequest<CreatePersonCommand.Args>(new(newPerson));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "firstName" && e.error == "First name is required.");
    }

    [Fact]
    public async Task CreatePersonCommand_ShouldFail_WhenFirstNameExceeds50Characters()
    {
        // Arrange
        var longFirstName = new string('A', 51);
        var newPerson = new NewPersonDto(
            FirstName: longFirstName,
            LastName: "Doe",
            Email: null,
            PhoneNumber: null,
            Location: null
        );
        var request = new AppRequest<CreatePersonCommand.Args>(new(newPerson));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "firstName" && e.error == "First name cannot exceed 50 characters.");
    }

    [Fact]
    public async Task CreatePersonCommand_ShouldFail_WhenLastNameIsEmpty()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: "",
            Email: null,
            PhoneNumber: null,
            Location: null
        );
        var request = new AppRequest<CreatePersonCommand.Args>(new(newPerson));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "lastName" && e.error == "Last name is required.");
    }

    [Fact]
    public async Task CreatePersonCommand_ShouldFail_WhenLastNameIsWhitespace()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: "   ",
            Email: null,
            PhoneNumber: null,
            Location: null
        );
        var request = new AppRequest<CreatePersonCommand.Args>(new(newPerson));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "lastName" && e.error == "Last name is required.");
    }

    [Fact]
    public async Task CreatePersonCommand_ShouldFail_WhenLastNameExceeds50Characters()
    {
        // Arrange
        var longLastName = new string('B', 51);
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: longLastName,
            Email: null,
            PhoneNumber: null,
            Location: null
        );
        var request = new AppRequest<CreatePersonCommand.Args>(new(newPerson));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "lastName" && e.error == "Last name cannot exceed 50 characters.");
    }

    [Fact]
    public async Task CreatePersonCommand_ShouldFail_WhenEmailIsInvalid()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: "Doe",
            Email: "invalid-email",
            PhoneNumber: null,
            Location: null
        );
        var request = new AppRequest<CreatePersonCommand.Args>(new(newPerson));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "email" && e.error == "Email is not a valid email address.");
    }

    [Fact]
    public async Task CreatePersonCommand_ShouldFail_WhenEmailExceeds100Characters()
    {
        // Arrange
        var longEmail = new string('a', 91) + "@email.com"; // Total 101 chars
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: "Doe",
            Email: longEmail,
            PhoneNumber: null,
            Location: null
        );
        var request = new AppRequest<CreatePersonCommand.Args>(new(newPerson));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "email" && e.error == "Email cannot exceed 100 characters.");
    }

    [Fact]
    public async Task CreatePersonCommand_ShouldFail_WhenPhoneNumberExceeds20Characters()
    {
        // Arrange
        var longPhoneNumber = new string('1', 21);
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: "Doe",
            Email: null,
            PhoneNumber: longPhoneNumber,
            Location: null
        );
        var request = new AppRequest<CreatePersonCommand.Args>(new(newPerson));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "phoneNumber" && e.error == "Phone number cannot exceed 20 characters.");
    }

    [Fact]
    public async Task CreatePersonCommand_ShouldFail_WhenLocationExceeds100Characters()
    {
        // Arrange
        var longLocation = new string('X', 101);
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: "Doe",
            Email: null,
            PhoneNumber: null,
            Location: longLocation
        );
        var request = new AppRequest<CreatePersonCommand.Args>(new(newPerson));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "location" && e.error == "Location cannot exceed 100 characters.");
    }

    [Fact]
    public async Task CreatePersonCommand_ShouldSucceed_WithValidEmail()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "Alice",
            LastName: "Johnson",
            Email: "alice.johnson@example.com",
            PhoneNumber: null,
            Location: null
        );
        var request = new AppRequest<CreatePersonCommand.Args>(new(newPerson));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("alice.johnson@example.com", result.Value!.CreatedPerson.Email);
    }

    [Fact]
    public async Task CreatePersonCommand_ShouldFail_WhenMultipleFieldsAreInvalid()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "",
            LastName: "",
            Email: "bad-email",
            PhoneNumber: new string('9', 21),
            Location: new string('Y', 101)
        );
        var request = new AppRequest<CreatePersonCommand.Args>(new(newPerson));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ValidationErrors);
        Assert.Equal(5, result.ValidationErrors.Count());
        Assert.Contains(result.ValidationErrors, e => e.field == "firstName");
        Assert.Contains(result.ValidationErrors, e => e.field == "lastName");
        Assert.Contains(result.ValidationErrors, e => e.field == "email");
        Assert.Contains(result.ValidationErrors, e => e.field == "phoneNumber");
        Assert.Contains(result.ValidationErrors, e => e.field == "location");
    }
}

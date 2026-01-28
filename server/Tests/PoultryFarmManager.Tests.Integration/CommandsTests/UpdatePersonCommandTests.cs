using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.Persons;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models.Finance;
using Xunit;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class UpdatePersonCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<UpdatePersonCommand.Args, UpdatePersonCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<UpdatePersonCommand.Args, UpdatePersonCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UpdatePersonCommand_ShouldUpdatePerson_WhenAllFieldsAreProvided()
    {
        // Arrange - Create a person in the database
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "1234567890",
            Location = "New York"
        };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        var updateData = new UpdatePersonDto(
            FirstName: "Jane",
            LastName: "Smith",
            Email: "jane@test.com",
            PhoneNumber: "0987654321",
            Location: "Los Angeles");

        var request = new AppRequest<UpdatePersonCommand.Args>(new(person.Id, updateData));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.UpdatedPerson);
        Assert.Equal(person.Id, result.Value!.UpdatedPerson.Id);
        Assert.Equal("Jane", result.Value!.UpdatedPerson.FirstName);
        Assert.Equal("Smith", result.Value!.UpdatedPerson.LastName);
        Assert.Equal("jane@test.com", result.Value!.UpdatedPerson.Email);
        Assert.Equal("0987654321", result.Value!.UpdatedPerson.PhoneNumber);
        Assert.Equal("Los Angeles", result.Value!.UpdatedPerson.Location);
    }

    [Fact]
    public async Task UpdatePersonCommand_ShouldUpdateOnlyFirstName_WhenOnlyFirstNameIsProvided()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "1234567890",
            Location = "New York"
        };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        var updateData = new UpdatePersonDto(
            FirstName: "Jane",
            LastName: null,
            Email: null,
            PhoneNumber: null,
            Location: null);

        var request = new AppRequest<UpdatePersonCommand.Args>(new(person.Id, updateData));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Jane", result.Value!.UpdatedPerson.FirstName);
        Assert.Equal("Doe", result.Value!.UpdatedPerson.LastName);
        Assert.Equal("john@test.com", result.Value!.UpdatedPerson.Email);
        Assert.Equal("1234567890", result.Value!.UpdatedPerson.PhoneNumber);
        Assert.Equal("New York", result.Value!.UpdatedPerson.Location);
    }

    [Fact]
    public async Task UpdatePersonCommand_ShouldUpdateOnlyLastName_WhenOnlyLastNameIsProvided()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "1234567890",
            Location = "New York"
        };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        var updateData = new UpdatePersonDto(
            FirstName: null,
            LastName: "Smith",
            Email: null,
            PhoneNumber: null,
            Location: null);

        var request = new AppRequest<UpdatePersonCommand.Args>(new(person.Id, updateData));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("John", result.Value!.UpdatedPerson.FirstName);
        Assert.Equal("Smith", result.Value!.UpdatedPerson.LastName);
        Assert.Equal("john@test.com", result.Value!.UpdatedPerson.Email);
        Assert.Equal("1234567890", result.Value!.UpdatedPerson.PhoneNumber);
        Assert.Equal("New York", result.Value!.UpdatedPerson.Location);
    }

    [Fact]
    public async Task UpdatePersonCommand_ShouldClearEmail_WhenEmptyStringIsProvided()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "1234567890",
            Location = "New York"
        };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        var updateData = new UpdatePersonDto(
            FirstName: null,
            LastName: null,
            Email: "",
            PhoneNumber: null,
            Location: null);

        var request = new AppRequest<UpdatePersonCommand.Args>(new(person.Id, updateData));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.UpdatedPerson.Email);
    }

    [Fact]
    public async Task UpdatePersonCommand_ShouldClearPhoneNumber_WhenEmptyStringIsProvided()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "1234567890",
            Location = "New York"
        };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        var updateData = new UpdatePersonDto(
            FirstName: null,
            LastName: null,
            Email: null,
            PhoneNumber: "",
            Location: null);

        var request = new AppRequest<UpdatePersonCommand.Args>(new(person.Id, updateData));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.UpdatedPerson.PhoneNumber);
    }

    [Fact]
    public async Task UpdatePersonCommand_ShouldClearLocation_WhenEmptyStringIsProvided()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PhoneNumber = "1234567890",
            Location = "New York"
        };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        var updateData = new UpdatePersonDto(
            FirstName: null,
            LastName: null,
            Email: null,
            PhoneNumber: null,
            Location: "");

        var request = new AppRequest<UpdatePersonCommand.Args>(new(person.Id, updateData));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.UpdatedPerson.Location);
    }

    [Fact]
    public async Task UpdatePersonCommand_ShouldReturnValidationError_WhenFirstNameExceeds100Characters()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        var updateData = new UpdatePersonDto(
            FirstName: new string('A', 101),
            LastName: null,
            Email: null,
            PhoneNumber: null,
            Location: null);

        var request = new AppRequest<UpdatePersonCommand.Args>(new(person.Id, updateData));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "firstName" && e.error.Contains("cannot exceed 100 characters"));
    }

    [Fact]
    public async Task UpdatePersonCommand_ShouldReturnValidationError_WhenLastNameExceeds100Characters()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        var updateData = new UpdatePersonDto(
            FirstName: null,
            LastName: new string('A', 101),
            Email: null,
            PhoneNumber: null,
            Location: null);

        var request = new AppRequest<UpdatePersonCommand.Args>(new(person.Id, updateData));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "lastName" && e.error.Contains("cannot exceed 100 characters"));
    }

    [Fact]
    public async Task UpdatePersonCommand_ShouldReturnValidationError_WhenEmailExceeds100Characters()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        var updateData = new UpdatePersonDto(
            FirstName: null,
            LastName: null,
            Email: new string('A', 101),
            PhoneNumber: null,
            Location: null);

        var request = new AppRequest<UpdatePersonCommand.Args>(new(person.Id, updateData));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "email" && e.error.Contains("cannot exceed 100 characters"));
    }

    [Fact]
    public async Task UpdatePersonCommand_ShouldReturnValidationError_WhenPhoneNumberExceeds20Characters()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        var updateData = new UpdatePersonDto(
            FirstName: null,
            LastName: null,
            Email: null,
            PhoneNumber: new string('1', 21),
            Location: null);

        var request = new AppRequest<UpdatePersonCommand.Args>(new(person.Id, updateData));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "phoneNumber" && e.error.Contains("cannot exceed 20 characters"));
    }

    [Fact]
    public async Task UpdatePersonCommand_ShouldReturnValidationError_WhenLocationExceeds100Characters()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        var updateData = new UpdatePersonDto(
            FirstName: null,
            LastName: null,
            Email: null,
            PhoneNumber: null,
            Location: new string('A', 101));

        var request = new AppRequest<UpdatePersonCommand.Args>(new(person.Id, updateData));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "location" && e.error.Contains("cannot exceed 100 characters"));
    }

    [Fact]
    public async Task UpdatePersonCommand_ShouldThrowInvalidOperationException_WhenPersonDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        var updateData = new UpdatePersonDto(
            FirstName: "Jane",
            LastName: null,
            Email: null,
            PhoneNumber: null,
            Location: null);

        var request = new AppRequest<UpdatePersonCommand.Args>(new(nonExistentId, updateData));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(request, CancellationToken.None));
        Assert.Contains("not found", exception.Message);
    }
}

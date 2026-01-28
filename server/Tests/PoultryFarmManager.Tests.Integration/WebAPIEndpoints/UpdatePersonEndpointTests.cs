using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Core.Models.Finance;
using Xunit;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class UpdatePersonEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task PUT_UpdatePerson_ShouldReturn200_WhenPersonIsUpdatedSuccessfully()
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
            LastName: "Smith",
            Email: "jane@test.com",
            PhoneNumber: "0987654321",
            Location: "Los Angeles");

        // Act
        var response = await fixture.Client.PutAsJsonAsync(
            $"/api/v1/persons/{person.Id}",
            new { Person = updateData });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UpdatePersonResponseBody>();
        Assert.NotNull(result);
        Assert.NotNull(result!.Person);
        Assert.Equal(person.Id, result.Person.Id);
        Assert.Equal("Jane", result.Person.FirstName);
        Assert.Equal("Smith", result.Person.LastName);
        Assert.Equal("jane@test.com", result.Person.Email);
        Assert.Equal("0987654321", result.Person.PhoneNumber);
        Assert.Equal("Los Angeles", result.Person.Location);
    }

    [Fact]
    public async Task PUT_UpdatePerson_ShouldReturn200_WhenOnlyFirstNameIsUpdated()
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

        // Act
        var response = await fixture.Client.PutAsJsonAsync(
            $"/api/v1/persons/{person.Id}",
            new { Person = updateData });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UpdatePersonResponseBody>();
        Assert.NotNull(result);
        Assert.Equal("Jane", result!.Person.FirstName);
        Assert.Equal("Doe", result.Person.LastName);
        Assert.Equal("john@test.com", result.Person.Email);
    }

    [Fact]
    public async Task PUT_UpdatePerson_ShouldReturn200_WhenClearingOptionalFields()
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
            PhoneNumber: "",
            Location: "");

        // Act
        var response = await fixture.Client.PutAsJsonAsync(
            $"/api/v1/persons/{person.Id}",
            new { Person = updateData });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<UpdatePersonResponseBody>();
        Assert.NotNull(result);
        Assert.Null(result!.Person.Email);
        Assert.Null(result.Person.PhoneNumber);
        Assert.Null(result.Person.Location);
    }

    [Fact]
    public async Task PUT_UpdatePerson_ShouldReturn400_WhenFirstNameExceeds100Characters()
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

        // Act
        var response = await fixture.Client.PutAsJsonAsync(
            $"/api/v1/persons/{person.Id}",
            new { Person = updateData });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("First name cannot exceed 100 characters", errorContent);
    }

    [Fact]
    public async Task PUT_UpdatePerson_ShouldReturn400_WhenLastNameExceeds100Characters()
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

        // Act
        var response = await fixture.Client.PutAsJsonAsync(
            $"/api/v1/persons/{person.Id}",
            new { Person = updateData });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Last name cannot exceed 100 characters", errorContent);
    }

    [Fact]
    public async Task PUT_UpdatePerson_ShouldReturn404_WhenPersonDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateData = new UpdatePersonDto(
            FirstName: "Jane",
            LastName: null,
            Email: null,
            PhoneNumber: null,
            Location: null);

        // Act
        var response = await fixture.Client.PutAsJsonAsync(
            $"/api/v1/persons/{nonExistentId}",
            new { Person = updateData });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("not found", errorContent);
    }

    private record UpdatePersonResponseBody(PersonDto Person);
}

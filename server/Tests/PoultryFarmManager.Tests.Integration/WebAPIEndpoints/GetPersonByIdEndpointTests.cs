using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Models.Finance;
using PoultryFarmManager.WebAPI.Endpoints.v1.Persons;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetPersonByIdEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_PersonById_ShouldReturnPerson()
    {
        // Arrange - Create test person
        var person = new Person
        {
            FirstName = "Michael",
            LastName = "Brown",
            Email = "michael.brown@example.com",
            PhoneNumber = "4445556666",
            Location = "San Francisco"
        };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/persons/{person.Id}");
        var responseBody = await response.Content.ReadFromJsonAsync<GetPersonByIdEndpoint.GetPersonByIdResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Person);
        Assert.Equal(person.Id, responseBody.Person.Id);
        Assert.Equal("Michael", responseBody.Person.FirstName);
        Assert.Equal("Brown", responseBody.Person.LastName);
        Assert.Equal("michael.brown@example.com", responseBody.Person.Email);
        Assert.Equal("4445556666", responseBody.Person.PhoneNumber);
        Assert.Equal("San Francisco", responseBody.Person.Location);
    }

    [Fact]
    public async Task GET_PersonById_NonExistentId_ShouldReturnNotFound()
    {
        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/persons/{Guid.NewGuid()}");

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GET_PersonById_ShouldReturnPerson_WithNullOptionalFields()
    {
        // Arrange - Create test person with null optional fields
        var person = new Person
        {
            FirstName = "Sarah",
            LastName = "Davis",
            Email = null,
            PhoneNumber = null,
            Location = null
        };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/persons/{person.Id}");
        var responseBody = await response.Content.ReadFromJsonAsync<GetPersonByIdEndpoint.GetPersonByIdResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Person);
        Assert.Equal(person.Id, responseBody.Person.Id);
        Assert.Equal("Sarah", responseBody.Person.FirstName);
        Assert.Equal("Davis", responseBody.Person.LastName);
        Assert.Null(responseBody.Person.Email);
        Assert.Null(responseBody.Person.PhoneNumber);
        Assert.Null(responseBody.Person.Location);
    }
}

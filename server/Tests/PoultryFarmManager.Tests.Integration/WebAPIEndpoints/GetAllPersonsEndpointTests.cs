using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Models.Finance;
using PoultryFarmManager.WebAPI.Endpoints.v1.Persons;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetAllPersonsEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_Persons_ShouldReturnAllPersons()
    {
        // Arrange - Create test persons
        var persons = new[]
        {
            new Person
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "1234567890",
                Location = "New York"
            },
            new Person
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                PhoneNumber = "0987654321",
                Location = "Los Angeles"
            }
        };
        dbContext.Persons.AddRange(persons);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await fixture.Client.GetAsync("/api/v1/persons");
        var responseBody = await response.Content.ReadFromJsonAsync<GetAllPersonsEndpoint.GetAllPersonsResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Persons);
        Assert.Equal(2, responseBody.Persons.Count());
        Assert.Contains(responseBody.Persons, p => p.FirstName == "John" && p.LastName == "Doe");
        Assert.Contains(responseBody.Persons, p => p.FirstName == "Jane" && p.LastName == "Smith");
    }

    [Fact]
    public async Task GET_Persons_ShouldReturnEmptyList_WhenNoPersons()
    {
        // Act
        var response = await fixture.Client.GetAsync("/api/v1/persons");
        var responseBody = await response.Content.ReadFromJsonAsync<GetAllPersonsEndpoint.GetAllPersonsResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Persons);
        Assert.Empty(responseBody.Persons);
    }

    [Fact]
    public async Task GET_Persons_ShouldReturnPersonsWithCorrectData()
    {
        // Arrange - Create test person
        var person = new Person
        {
            FirstName = "Alice",
            LastName = "Williams",
            Email = "alice.williams@example.com",
            PhoneNumber = "1112223333",
            Location = "Chicago"
        };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await fixture.Client.GetAsync("/api/v1/persons");
        var responseBody = await response.Content.ReadFromJsonAsync<GetAllPersonsEndpoint.GetAllPersonsResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Persons);
        Assert.Single(responseBody.Persons);

        var returnedPerson = responseBody.Persons.First();
        Assert.Equal(person.Id, returnedPerson.Id);
        Assert.Equal("Alice", returnedPerson.FirstName);
        Assert.Equal("Williams", returnedPerson.LastName);
        Assert.Equal("alice.williams@example.com", returnedPerson.Email);
        Assert.Equal("1112223333", returnedPerson.PhoneNumber);
        Assert.Equal("Chicago", returnedPerson.Location);
    }
}

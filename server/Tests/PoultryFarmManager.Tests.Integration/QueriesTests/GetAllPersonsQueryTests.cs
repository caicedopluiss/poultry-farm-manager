using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.Persons;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetAllPersonsQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetAllPersonsQuery.Args, GetAllPersonsQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetAllPersonsQuery.Args, GetAllPersonsQuery.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAllPersonsQuery_ShouldReturnAllPersons()
    {
        // Arrange - Create multiple persons
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
            },
            new Person
            {
                FirstName = "Bob",
                LastName = "Johnson",
                Email = null,
                PhoneNumber = "5555555555",
                Location = null
            }
        };
        dbContext.Persons.AddRange(persons);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetAllPersonsQuery.Args>(new());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value!.Persons.Count);
        Assert.Contains(result.Value!.Persons, p => p.FirstName == "John" && p.LastName == "Doe");
        Assert.Contains(result.Value!.Persons, p => p.FirstName == "Jane" && p.LastName == "Smith");
        Assert.Contains(result.Value!.Persons, p => p.FirstName == "Bob" && p.LastName == "Johnson");
    }

    [Fact]
    public async Task GetAllPersonsQuery_ShouldReturnEmptyList_WhenNoPersons()
    {
        // Arrange
        var request = new AppRequest<GetAllPersonsQuery.Args>(new());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.Persons);
    }

    [Fact]
    public async Task GetAllPersonsQuery_ShouldReturnPersonsWithCorrectData()
    {
        // Arrange
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

        var request = new AppRequest<GetAllPersonsQuery.Args>(new());

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value!.Persons);

        var returnedPerson = result.Value!.Persons.First();
        Assert.Equal(person.Id, returnedPerson.Id);
        Assert.Equal("Alice", returnedPerson.FirstName);
        Assert.Equal("Williams", returnedPerson.LastName);
        Assert.Equal("alice.williams@example.com", returnedPerson.Email);
        Assert.Equal("1112223333", returnedPerson.PhoneNumber);
        Assert.Equal("Chicago", returnedPerson.Location);
    }
}

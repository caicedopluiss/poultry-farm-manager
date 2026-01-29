using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.Persons;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetPersonByIdQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetPersonByIdQuery.Args, GetPersonByIdQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetPersonByIdQuery.Args, GetPersonByIdQuery.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetPersonByIdQuery_ShouldReturnPerson_WhenExists()
    {
        // Arrange - Create a person
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

        var request = new AppRequest<GetPersonByIdQuery.Args>(new(person.Id));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.Person);
        Assert.Equal(person.Id, result.Value!.Person.Id);
        Assert.Equal("Michael", result.Value!.Person.FirstName);
        Assert.Equal("Brown", result.Value!.Person.LastName);
        Assert.Equal("michael.brown@example.com", result.Value!.Person.Email);
        Assert.Equal("4445556666", result.Value!.Person.PhoneNumber);
        Assert.Equal("San Francisco", result.Value!.Person.Location);
    }

    [Fact]
    public async Task GetPersonByIdQuery_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var request = new AppRequest<GetPersonByIdQuery.Args>(new(Guid.NewGuid()));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value!.Person);
    }

    [Fact]
    public async Task GetPersonByIdQuery_ShouldReturnPerson_WithNullOptionalFields()
    {
        // Arrange - Create a person with null optional fields
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

        var request = new AppRequest<GetPersonByIdQuery.Args>(new(person.Id));

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.Person);
        Assert.Equal(person.Id, result.Value!.Person.Id);
        Assert.Equal("Sarah", result.Value!.Person.FirstName);
        Assert.Equal("Davis", result.Value!.Person.LastName);
        Assert.Null(result.Value!.Person.Email);
        Assert.Null(result.Value!.Person.PhoneNumber);
        Assert.Null(result.Value!.Person.Location);
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.Vendors;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetAllVendorsQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetAllVendorsQuery.Args, GetAllVendorsQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetAllVendorsQuery.Args, GetAllVendorsQuery.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAllVendorsQuery_ShouldReturnAllVendors()
    {
        // Arrange
        var contactPerson1 = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe"
        };
        var contactPerson2 = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith"
        };
        await dbContext.Persons.AddRangeAsync(contactPerson1, contactPerson2);

        var vendor1 = new Vendor
        {
            Id = Guid.NewGuid(),
            Name = "Acme Corp",
            Location = "Downtown",
            ContactPersonId = contactPerson1.Id
        };
        var vendor2 = new Vendor
        {
            Id = Guid.NewGuid(),
            Name = "Best Supplies",
            Location = "Uptown",
            ContactPersonId = contactPerson2.Id
        };
        await dbContext.Vendors.AddRangeAsync(vendor1, vendor2);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetAllVendorsQuery.Args>(new());

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Vendors.Count());
        Assert.Contains(result.Value!.Vendors, v => v.Name == "Acme Corp");
        Assert.Contains(result.Value!.Vendors, v => v.Name == "Best Supplies");
    }

    [Fact]
    public async Task GetAllVendorsQuery_ShouldReturnEmptyList_WhenNoVendorsExist()
    {
        // Arrange
        var request = new AppRequest<GetAllVendorsQuery.Args>(new());

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.Vendors);
    }

    [Fact]
    public async Task GetAllVendorsQuery_ShouldIncludeContactPersonData()
    {
        // Arrange
        var contactPerson = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };
        await dbContext.Persons.AddAsync(contactPerson);

        var vendor = new Vendor
        {
            Id = Guid.NewGuid(),
            Name = "Test Vendor",
            ContactPersonId = contactPerson.Id
        };
        await dbContext.Vendors.AddAsync(vendor);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetAllVendorsQuery.Args>(new());

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var vendorDto = result.Value!.Vendors.First();
        Assert.NotNull(vendorDto.ContactPerson);
        Assert.Equal("John", vendorDto.ContactPerson!.FirstName);
        Assert.Equal("Doe", vendorDto.ContactPerson!.LastName);
        Assert.Equal("john@example.com", vendorDto.ContactPerson!.Email);
    }
}

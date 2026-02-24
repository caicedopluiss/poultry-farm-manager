using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Queries.Vendors;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Tests.Integration.QueriesTests;

public class GetVendorByIdQueryTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<GetVendorByIdQuery.Args, GetVendorByIdQuery.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<GetVendorByIdQuery.Args, GetVendorByIdQuery.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetVendorByIdQuery_ShouldReturnVendor_WhenExists()
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
            Name = "Acme Corp",
            Location = "Downtown",
            ContactPersonId = contactPerson.Id
        };
        await dbContext.Vendors.AddAsync(vendor);
        await dbContext.SaveChangesAsync();

        var request = new AppRequest<GetVendorByIdQuery.Args>(new(vendor.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.Vendor);
        Assert.Equal(vendor.Id, result.Value!.Vendor!.Id);
        Assert.Equal("Acme Corp", result.Value!.Vendor!.Name);
        Assert.Equal("Downtown", result.Value!.Vendor!.Location);
        Assert.Equal(contactPerson.Id, result.Value!.Vendor!.ContactPersonId);
    }

    [Fact]
    public async Task GetVendorByIdQuery_ShouldIncludeContactPersonData()
    {
        // Arrange
        var contactPerson = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith",
            PhoneNumber = "555-0100"
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

        var request = new AppRequest<GetVendorByIdQuery.Args>(new(vendor.Id));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.Vendor!.ContactPerson);
        Assert.Equal("Jane", result.Value!.Vendor!.ContactPerson!.FirstName);
        Assert.Equal("Smith", result.Value!.Vendor!.ContactPerson!.LastName);
        Assert.Equal("555-0100", result.Value!.Vendor!.ContactPerson!.PhoneNumber);
    }

    [Fact]
    public async Task GetVendorByIdQuery_ShouldReturnNull_WhenVendorNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new AppRequest<GetVendorByIdQuery.Args>(new(nonExistentId));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.Vendor);
    }
}

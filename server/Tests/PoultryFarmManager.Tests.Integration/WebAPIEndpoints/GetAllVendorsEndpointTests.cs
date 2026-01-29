using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Models.Finance;
using PoultryFarmManager.WebAPI.Endpoints.v1.Vendors;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetAllVendorsEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_Vendors_ShouldReturnAllVendors()
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

        // Act
        var response = await fixture.Client.GetAsync("/api/v1/vendors");
        var responseBody = await response.Content.ReadFromJsonAsync<GetAllVendorsEndpoint.GetAllVendorsResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Equal(2, responseBody.Vendors.Count());
        Assert.Contains(responseBody.Vendors, v => v.Name == "Acme Corp");
        Assert.Contains(responseBody.Vendors, v => v.Name == "Best Supplies");
    }

    [Fact]
    public async Task GET_Vendors_ShouldReturnEmptyArray_WhenNoVendorsExist()
    {
        // Act
        var response = await fixture.Client.GetAsync("/api/v1/vendors");
        var responseBody = await response.Content.ReadFromJsonAsync<GetAllVendorsEndpoint.GetAllVendorsResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(responseBody);
        Assert.Empty(responseBody.Vendors);
    }

    [Fact]
    public async Task GET_Vendors_ShouldIncludeContactPersonData()
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

        // Act
        var response = await fixture.Client.GetAsync("/api/v1/vendors");
        var responseBody = await response.Content.ReadFromJsonAsync<GetAllVendorsEndpoint.GetAllVendorsResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var vendorDto = responseBody!.Vendors.First();
        Assert.NotNull(vendorDto.ContactPerson);
        Assert.Equal("John", vendorDto.ContactPerson!.FirstName);
        Assert.Equal("john@example.com", vendorDto.ContactPerson!.Email);
    }
}

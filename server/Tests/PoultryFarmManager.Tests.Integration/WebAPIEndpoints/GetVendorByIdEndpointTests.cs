using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Core.Models.Finance;
using PoultryFarmManager.WebAPI.Endpoints.v1.Vendors;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class GetVendorByIdEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task GET_VendorById_ExistingVendor_ShouldReturnOk()
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

        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/vendors/{vendor.Id}");
        var responseBody = await response.Content.ReadFromJsonAsync<GetVendorByIdEndpoint.GetVendorByIdResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Equal(vendor.Id, responseBody.Vendor.Id);
        Assert.Equal("Acme Corp", responseBody.Vendor.Name);
        Assert.Equal("Downtown", responseBody.Vendor.Location);
        Assert.Equal(contactPerson.Id, responseBody.Vendor.ContactPersonId);
    }

    [Fact]
    public async Task GET_VendorById_ShouldIncludeContactPersonData()
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

        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/vendors/{vendor.Id}");
        var responseBody = await response.Content.ReadFromJsonAsync<GetVendorByIdEndpoint.GetVendorByIdResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(responseBody!.Vendor.ContactPerson);
        Assert.Equal("Jane", responseBody.Vendor.ContactPerson!.FirstName);
        Assert.Equal("Smith", responseBody.Vendor.ContactPerson!.LastName);
        Assert.Equal("555-0100", responseBody.Vendor.ContactPerson!.PhoneNumber);
    }

    [Fact]
    public async Task GET_VendorById_NonExistentVendor_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await fixture.Client.GetAsync($"/api/v1/vendors/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Core.Models.Finance;
using PoultryFarmManager.WebAPI;
using PoultryFarmManager.WebAPI.Endpoints.v1.Vendors;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class UpdateVendorEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task PUT_Vendor_ValidUpdate_ShouldReturnOk()
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

        var vendor = new Vendor
        {
            Id = Guid.NewGuid(),
            Name = "Old Name",
            Location = "Old Location",
            ContactPersonId = contactPerson1.Id
        };
        await dbContext.Vendors.AddAsync(vendor);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateVendorDto(
            Name: "New Name",
            Location: "New Location",
            ContactPersonId: contactPerson2.Id
        );
        var body = new { vendor = updateDto };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/vendors/{vendor.Id}", body);
        var responseBody = await response.Content.ReadFromJsonAsync<UpdateVendorEndpoint.UpdateVendorResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Equal("New Name", responseBody.Vendor.Name);
        Assert.Equal("New Location", responseBody.Vendor.Location);
        Assert.Equal(contactPerson2.Id, responseBody.Vendor.ContactPersonId);
    }

    [Fact]
    public async Task PUT_Vendor_NonExistentVendor_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateDto = new UpdateVendorDto(
            Name: "New Name",
            Location: null,
            ContactPersonId: null
        );
        var body = new { vendor = updateDto };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/vendors/{nonExistentId}", body);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PUT_Vendor_EmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var contactPerson = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe"
        };
        await dbContext.Persons.AddAsync(contactPerson);

        var vendor = new Vendor
        {
            Id = Guid.NewGuid(),
            Name = "Original",
            ContactPersonId = contactPerson.Id
        };
        await dbContext.Vendors.AddAsync(vendor);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateVendorDto(
            Name: "",
            Location: null,
            ContactPersonId: null
        );
        var body = new { vendor = updateDto };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/vendors/{vendor.Id}", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(errorResponse);
        Assert.Contains(errorResponse.ValidationErrors, e => e.FieldName == "name");
    }

    [Fact]
    public async Task PUT_Vendor_NonExistentContactPerson_ShouldReturnBadRequest()
    {
        // Arrange
        var contactPerson = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe"
        };
        await dbContext.Persons.AddAsync(contactPerson);

        var vendor = new Vendor
        {
            Id = Guid.NewGuid(),
            Name = "Vendor",
            ContactPersonId = contactPerson.Id
        };
        await dbContext.Vendors.AddAsync(vendor);
        await dbContext.SaveChangesAsync();

        var nonExistentPersonId = Guid.NewGuid();
        var updateDto = new UpdateVendorDto(
            Name: null,
            Location: null,
            ContactPersonId: nonExistentPersonId
        );
        var body = new { vendor = updateDto };

        // Act
        var response = await fixture.Client.PutAsJsonAsync($"/api/v1/vendors/{vendor.Id}", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(errorResponse);
        Assert.Contains(errorResponse.ValidationErrors, e => e.FieldName == "contactPersonId");
    }
}

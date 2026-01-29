using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Core.Models.Finance;
using PoultryFarmManager.WebAPI;
using PoultryFarmManager.WebAPI.Endpoints.v1.Vendors;
using Xunit;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class CreateVendorEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task POST_Vendor_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var newVendor = new NewVendorDto(
            Name: "ABC Suppliers",
            Location: "123 Main St",
            ContactPersonId: contactPerson.Id
        );
        var body = new { vendor = newVendor };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/vendors", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreateVendorEndpoint.CreateVendorResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Vendor);
        Assert.NotEqual(Guid.Empty, responseBody.Vendor.Id);
        Assert.Equal("ABC Suppliers", responseBody.Vendor.Name);
        Assert.Equal("123 Main St", responseBody.Vendor.Location);
        Assert.Equal(contactPerson.Id, responseBody.Vendor.ContactPersonId);
        Assert.NotNull(responseBody.Vendor.ContactPerson);
        Assert.Equal($"/api/v1/vendors/{responseBody.Vendor.Id}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task POST_Vendor_WithMinimumFields_ShouldReturnCreated()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "Jane",
            LastName = "Smith"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var newVendor = new NewVendorDto(
            Name: "XYZ Company",
            Location: null,
            ContactPersonId: contactPerson.Id
        );
        var body = new { vendor = newVendor };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/vendors", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreateVendorEndpoint.CreateVendorResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Vendor);
        Assert.Equal("XYZ Company", responseBody.Vendor.Name);
        Assert.Null(responseBody.Vendor.Location);
    }

    [Fact]
    public async Task POST_Vendor_EmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "Test",
            LastName = "User"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var newVendor = new NewVendorDto(
            Name: "",
            Location: null,
            ContactPersonId: contactPerson.Id
        );
        var body = new { vendor = newVendor };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/vendors", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "name" && e.Errors.Contains("Vendor name is required."));
    }

    [Fact]
    public async Task POST_Vendor_WhitespaceName_ShouldReturnBadRequest()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "Test",
            LastName = "User"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var newVendor = new NewVendorDto(
            Name: "   ",
            Location: null,
            ContactPersonId: contactPerson.Id
        );
        var body = new { vendor = newVendor };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/vendors", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "name" && e.Errors.Contains("Vendor name is required."));
    }

    [Fact]
    public async Task POST_Vendor_NameTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "Test",
            LastName = "User"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var longName = new string('A', 101);
        var newVendor = new NewVendorDto(
            Name: longName,
            Location: null,
            ContactPersonId: contactPerson.Id
        );
        var body = new { vendor = newVendor };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/vendors", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "name" && e.Errors.Contains("Vendor name cannot exceed 100 characters."));
    }

    [Fact]
    public async Task POST_Vendor_LocationTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "Test",
            LastName = "User"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var longLocation = new string('A', 101);
        var newVendor = new NewVendorDto(
            Name: "Valid Name",
            Location: longLocation,
            ContactPersonId: contactPerson.Id
        );
        var body = new { vendor = newVendor };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/vendors", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "location" && e.Errors.Contains("Location cannot exceed 100 characters."));
    }

    [Fact]
    public async Task POST_Vendor_EmptyContactPersonId_ShouldReturnBadRequest()
    {
        // Arrange
        var newVendor = new NewVendorDto(
            Name: "Valid Name",
            Location: null,
            ContactPersonId: Guid.Empty
        );
        var body = new { vendor = newVendor };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/vendors", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "contactPersonId" && e.Errors.Contains("Contact person is required."));
    }

    [Fact]
    public async Task POST_Vendor_NonExistentContactPerson_ShouldReturnBadRequest()
    {
        // Arrange
        var nonExistentPersonId = Guid.NewGuid();
        var newVendor = new NewVendorDto(
            Name: "Valid Name",
            Location: null,
            ContactPersonId: nonExistentPersonId
        );
        var body = new { vendor = newVendor };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/vendors", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "contactPersonId" && e.Errors.Contains("Contact person does not exist."));
    }

    [Fact]
    public async Task POST_Vendor_MultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var longName = new string('A', 101);
        var longLocation = new string('B', 101);
        var newVendor = new NewVendorDto(
            Name: longName,
            Location: longLocation,
            ContactPersonId: Guid.Empty
        );
        var body = new { vendor = newVendor };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/vendors", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "name" && e.Errors.Contains("Vendor name cannot exceed 100 characters."));
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "location" && e.Errors.Contains("Location cannot exceed 100 characters."));
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "contactPersonId" && e.Errors.Contains("Contact person is required."));
    }

    [Fact]
    public async Task POST_Vendor_Exactly100CharacterName_ShouldReturnCreated()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "Test",
            LastName = "User"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var exactName = new string('A', 100);
        var newVendor = new NewVendorDto(
            Name: exactName,
            Location: null,
            ContactPersonId: contactPerson.Id
        );
        var body = new { vendor = newVendor };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/vendors", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreateVendorEndpoint.CreateVendorResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Vendor);
        Assert.Equal(exactName, responseBody.Vendor.Name);
    }

    [Fact]
    public async Task POST_Vendor_Exactly100CharacterLocation_ShouldReturnCreated()
    {
        // Arrange
        var contactPerson = new Person
        {
            FirstName = "Test",
            LastName = "User"
        };
        dbContext.Persons.Add(contactPerson);
        await dbContext.SaveChangesAsync();

        var exactLocation = new string('A', 100);
        var newVendor = new NewVendorDto(
            Name: "Valid Name",
            Location: exactLocation,
            ContactPersonId: contactPerson.Id
        );
        var body = new { vendor = newVendor };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/vendors", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreateVendorEndpoint.CreateVendorResponseBody>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Vendor);
        Assert.Equal(exactLocation, responseBody.Vendor.Location);
    }
}

using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.WebAPI;
using PoultryFarmManager.WebAPI.Endpoints.v1.Persons;

namespace PoultryFarmManager.Tests.Integration.WebAPIEndpoints;

public class CreatePersonEndpointTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    [Fact]
    public async Task POST_Person_ValidPersonWithAllFields_ShouldReturnCreatedWithLocationHeader()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            PhoneNumber: "555-0100",
            Location: "Downtown Office"
        );
        var body = new { person = newPerson };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/persons", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreatePersonEndpoint.CreatePersonResponseBody>();

        // Assert - HTTP-specific concerns
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/api/v1/persons/", response.Headers.Location.ToString());

        // Response body structure
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Person);
        Assert.NotEqual(Guid.Empty, responseBody.Person.Id);
        Assert.Equal("John", responseBody.Person.FirstName);
        Assert.Equal("Doe", responseBody.Person.LastName);
        Assert.Equal("john.doe@example.com", responseBody.Person.Email);
        Assert.Equal("555-0100", responseBody.Person.PhoneNumber);
        Assert.Equal("Downtown Office", responseBody.Person.Location);
    }

    [Fact]
    public async Task POST_Person_ValidPersonWithOnlyRequiredFields_ShouldReturnCreated()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "Jane",
            LastName: "Smith",
            Email: null,
            PhoneNumber: null,
            Location: null
        );
        var body = new { person = newPerson };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/persons", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreatePersonEndpoint.CreatePersonResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(responseBody);
        Assert.Equal("Jane", responseBody.Person.FirstName);
        Assert.Equal("Smith", responseBody.Person.LastName);
        Assert.Null(responseBody.Person.Email);
        Assert.Null(responseBody.Person.PhoneNumber);
        Assert.Null(responseBody.Person.Location);
    }

    [Fact]
    public async Task POST_Person_EmptyFirstName_ShouldReturnBadRequest()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "",
            LastName: "Doe",
            Email: null,
            PhoneNumber: null,
            Location: null
        );
        var body = new { person = newPerson };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/persons", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(errorResponse);
        Assert.NotNull(errorResponse.ValidationErrors);
        Assert.Contains(errorResponse.ValidationErrors, e => e.FieldName == "firstName" && e.Errors.Contains("First name is required."));
    }

    [Fact]
    public async Task POST_Person_EmptyLastName_ShouldReturnBadRequest()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: "",
            Email: null,
            PhoneNumber: null,
            Location: null
        );
        var body = new { person = newPerson };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/persons", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "lastName" && e.Errors.Contains("Last name is required."));
    }

    [Fact]
    public async Task POST_Person_FirstNameTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var longFirstName = new string('A', 51);
        var newPerson = new NewPersonDto(
            FirstName: longFirstName,
            LastName: "Doe",
            Email: null,
            PhoneNumber: null,
            Location: null
        );
        var body = new { person = newPerson };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/persons", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "firstName" && e.Errors.Contains("First name cannot exceed 50 characters."));
    }

    [Fact]
    public async Task POST_Person_LastNameTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var longLastName = new string('B', 51);
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: longLastName,
            Email: null,
            PhoneNumber: null,
            Location: null
        );
        var body = new { person = newPerson };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/persons", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "lastName" && e.Errors.Contains("Last name cannot exceed 50 characters."));
    }

    [Fact]
    public async Task POST_Person_InvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: "Doe",
            Email: "invalid-email",
            PhoneNumber: null,
            Location: null
        );
        var body = new { person = newPerson };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/persons", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "email" && e.Errors.Contains("Email is not a valid email address."));
    }

    [Fact]
    public async Task POST_Person_EmailTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var longEmail = new string('a', 91) + "@email.com"; // Total 101 chars
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: "Doe",
            Email: longEmail,
            PhoneNumber: null,
            Location: null
        );
        var body = new { person = newPerson };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/persons", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "email" && e.Errors.Contains("Email cannot exceed 100 characters."));
    }

    [Fact]
    public async Task POST_Person_PhoneNumberTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var longPhoneNumber = new string('1', 21);
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: "Doe",
            Email: null,
            PhoneNumber: longPhoneNumber,
            Location: null
        );
        var body = new { person = newPerson };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/persons", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "phoneNumber" && e.Errors.Contains("Phone number cannot exceed 20 characters."));
    }

    [Fact]
    public async Task POST_Person_LocationTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var longLocation = new string('X', 101);
        var newPerson = new NewPersonDto(
            FirstName: "John",
            LastName: "Doe",
            Email: null,
            PhoneNumber: null,
            Location: longLocation
        );
        var body = new { person = newPerson };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/persons", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(errorResponse!.ValidationErrors, e => e.FieldName == "location" && e.Errors.Contains("Location cannot exceed 100 characters."));
    }

    [Fact]
    public async Task POST_Person_MultipleValidationErrors_ShouldReturnBadRequestWithAllErrors()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "",
            LastName: "",
            Email: "bad-email",
            PhoneNumber: new string('9', 21),
            Location: new string('Y', 101)
        );
        var body = new { person = newPerson };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/persons", body);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(errorResponse);
        Assert.NotNull(errorResponse.ValidationErrors);
        var validationErrorsList = errorResponse.ValidationErrors.ToList();
        Assert.Equal(5, validationErrorsList.Count);
        Assert.Contains(validationErrorsList, e => e.FieldName == "firstName");
        Assert.Contains(validationErrorsList, e => e.FieldName == "lastName");
        Assert.Contains(validationErrorsList, e => e.FieldName == "email");
        Assert.Contains(validationErrorsList, e => e.FieldName == "phoneNumber");
        Assert.Contains(validationErrorsList, e => e.FieldName == "location");
    }

    [Fact]
    public async Task POST_Person_ValidEmail_ShouldReturnCreated()
    {
        // Arrange
        var newPerson = new NewPersonDto(
            FirstName: "Alice",
            LastName: "Johnson",
            Email: "alice.johnson@example.com",
            PhoneNumber: null,
            Location: null
        );
        var body = new { person = newPerson };

        // Act
        var response = await fixture.Client.PostAsJsonAsync("/api/v1/persons", body);
        var responseBody = await response.Content.ReadFromJsonAsync<CreatePersonEndpoint.CreatePersonResponseBody>();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal("alice.johnson@example.com", responseBody!.Person.Email);
    }
}

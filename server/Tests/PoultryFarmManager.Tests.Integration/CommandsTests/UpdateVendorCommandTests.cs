using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.Vendors;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Tests.Integration.CommandsTests;

public class UpdateVendorCommandTests(TestsFixture fixture) : IClassFixture<TestsFixture>, IAsyncLifetime
{
    private readonly TestsDbContext dbContext = fixture.ServiceProvider.GetRequiredService<TestsDbContext>();
    private readonly IAppRequestHandler<UpdateVendorCommand.Args, UpdateVendorCommand.Result> handler = fixture.ServiceProvider.GetRequiredService<IAppRequestHandler<UpdateVendorCommand.Args, UpdateVendorCommand.Result>>();

    public async Task DisposeAsync() => await dbContext.ClearDbAsync();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UpdateVendorCommand_ShouldUpdateAllFields_Successfully()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N")[..8];
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
            Name = $"Old Name {testId}",
            Location = "Old Location",
            ContactPersonId = contactPerson1.Id
        };
        await dbContext.Vendors.AddAsync(vendor);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateVendorDto(
            Name: $"New Name {testId}",
            Location: "New Location",
            ContactPersonId: contactPerson2.Id
        );
        var request = new AppRequest<UpdateVendorCommand.Args>(new(vendor.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal($"New Name {testId}", result.Value!.UpdatedVendor.Name);
        Assert.Equal("New Location", result.Value!.UpdatedVendor.Location);
        Assert.Equal(contactPerson2.Id, result.Value!.UpdatedVendor.ContactPersonId);
        Assert.Equal("Jane", result.Value!.UpdatedVendor.ContactPerson!.FirstName);
    }

    [Fact]
    public async Task UpdateVendorCommand_ShouldUpdatePartially_OnlyName()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N")[..8];
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
            Name = $"Original Name {testId}",
            Location = "Original Location",
            ContactPersonId = contactPerson.Id
        };
        await dbContext.Vendors.AddAsync(vendor);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateVendorDto(
            Name: $"Updated Name {testId}",
            Location: null,
            ContactPersonId: null
        );
        var request = new AppRequest<UpdateVendorCommand.Args>(new(vendor.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal($"Updated Name {testId}", result.Value!.UpdatedVendor.Name);
        Assert.Equal("Original Location", result.Value!.UpdatedVendor.Location);
        Assert.Equal(contactPerson.Id, result.Value!.UpdatedVendor.ContactPersonId);
    }

    [Fact]
    public async Task UpdateVendorCommand_ShouldUpdatePartially_OnlyLocation()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N")[..8];
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
            Name = $"Vendor Name {testId}",
            Location = "Old Location",
            ContactPersonId = contactPerson.Id
        };
        await dbContext.Vendors.AddAsync(vendor);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateVendorDto(
            Name: null,
            Location: "New Location",
            ContactPersonId: null
        );
        var request = new AppRequest<UpdateVendorCommand.Args>(new(vendor.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal($"Vendor Name {testId}", result.Value!.UpdatedVendor.Name);
        Assert.Equal("New Location", result.Value!.UpdatedVendor.Location);
    }

    [Fact]
    public async Task UpdateVendorCommand_ShouldUpdatePartially_OnlyContactPerson()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N")[..8];
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
            Name = $"Vendor Name {testId}",
            Location = "Location",
            ContactPersonId = contactPerson1.Id
        };
        await dbContext.Vendors.AddAsync(vendor);
        await dbContext.SaveChangesAsync();

        var updateDto = new UpdateVendorDto(
            Name: null,
            Location: null,
            ContactPersonId: contactPerson2.Id
        );
        var request = new AppRequest<UpdateVendorCommand.Args>(new(vendor.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(contactPerson2.Id, result.Value!.UpdatedVendor.ContactPersonId);
        Assert.Equal("Jane", result.Value!.UpdatedVendor.ContactPerson!.FirstName);
    }

    [Fact]
    public async Task UpdateVendorCommand_ShouldFail_WhenVendorNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateDto = new UpdateVendorDto(
            Name: "New Name",
            Location: null,
            ContactPersonId: null
        );
        var request = new AppRequest<UpdateVendorCommand.Args>(new(nonExistentId, updateDto));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task UpdateVendorCommand_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N")[..8];
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
        var request = new AppRequest<UpdateVendorCommand.Args>(new(vendor.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "name" && e.error.Contains("empty"));
    }

    [Fact]
    public async Task UpdateVendorCommand_ShouldFail_WhenNameExceeds100Characters()
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

        var longName = new string('A', 101);
        var updateDto = new UpdateVendorDto(
            Name: longName,
            Location: null,
            ContactPersonId: null
        );
        var request = new AppRequest<UpdateVendorCommand.Args>(new(vendor.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "name" && e.error.Contains("100 characters"));
    }

    [Fact]
    public async Task UpdateVendorCommand_ShouldFail_WhenContactPersonDoesNotExist()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N")[..8];
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
            Name = $"Vendor {testId}",
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
        var request = new AppRequest<UpdateVendorCommand.Args>(new(vendor.Id, updateDto));

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.ValidationErrors, e => e.field == "contactPersonId" && e.error.Contains("does not exist"));
    }
}

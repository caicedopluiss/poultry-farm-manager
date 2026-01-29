using System;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Application.DTOs;

public record NewVendorDto(
    string Name,
    string? Location,
    Guid ContactPersonId)
{
    public Vendor Map()
    {
        return new Vendor
        {
            Id = Guid.NewGuid(),
            Name = Name,
            Location = Location,
            ContactPersonId = ContactPersonId
        };
    }
}

public record VendorDto(
    Guid Id,
    string Name,
    string? Location,
    Guid ContactPersonId,
    PersonDto? ContactPerson)
{
    public VendorDto() : this(Guid.Empty, string.Empty, null, Guid.Empty, null) { }

    public VendorDto Map(Vendor vendor)
    {
        PersonDto? contactPersonDto = null;
        if (vendor.ContactPerson != null)
        {
            contactPersonDto = new PersonDto().Map(vendor.ContactPerson);
        }

        return new VendorDto(
            Id: vendor.Id,
            Name: vendor.Name,
            Location: vendor.Location,
            ContactPersonId: vendor.ContactPersonId,
            ContactPerson: contactPersonDto
        );
    }
}

public record UpdateVendorDto(
    string? Name,
    string? Location,
    Guid? ContactPersonId)
{
    public void ApplyTo(Vendor vendor)
    {
        if (Name is not null)
            vendor.Name = Name;

        if (Location is not null)
            vendor.Location = Location;

        if (ContactPersonId is not null)
            vendor.ContactPersonId = ContactPersonId.Value;
    }
}

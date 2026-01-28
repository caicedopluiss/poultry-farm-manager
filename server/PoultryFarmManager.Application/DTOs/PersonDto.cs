using System;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Application.DTOs;

public record NewPersonDto(
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? Location)
{
    public Person Map(Person? to = null)
    {
        var result = to ?? new();

        result.FirstName = FirstName;
        result.LastName = LastName;
        result.Email = Email;
        result.PhoneNumber = PhoneNumber;
        result.Location = Location;

        return result;
    }
}

public record PersonDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? Location
)
{
    /// <summary>
    /// Parameterless constructor for mapping dto instance from core model/entity.
    /// </summary>
    public PersonDto() : this(
        Guid.Empty,
        string.Empty,
        string.Empty,
        null,
        null,
        null)
    {
    }

    public PersonDto Map(Person from)
    {
        return this with
        {
            Id = from.Id,
            FirstName = from.FirstName,
            LastName = from.LastName,
            Email = from.Email,
            PhoneNumber = from.PhoneNumber,
            Location = from.Location
        };
    }
}

public record UpdatePersonDto(
    string? FirstName,
    string? LastName,
    string? Email,
    string? PhoneNumber,
    string? Location)
{
    public void ApplyTo(Person person)
    {
        if (!string.IsNullOrWhiteSpace(FirstName))
        {
            person.FirstName = FirstName;
        }

        if (!string.IsNullOrWhiteSpace(LastName))
        {
            person.LastName = LastName;
        }

        if (Email != null)
        {
            person.Email = string.IsNullOrWhiteSpace(Email) ? null : Email;
        }

        if (PhoneNumber != null)
        {
            person.PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber;
        }

        if (Location != null)
        {
            person.Location = string.IsNullOrWhiteSpace(Location) ? null : Location;
        }
    }
}

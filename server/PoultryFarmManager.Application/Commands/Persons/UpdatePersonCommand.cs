using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Commands.Persons;

public static class UpdatePersonCommand
{
    public record Args(
        Guid PersonId,
        UpdatePersonDto UpdateData);

    public record Result(PersonDto UpdatedPerson);

    public class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            // Validate first name (if provided)
            if (!string.IsNullOrWhiteSpace(args.UpdateData.FirstName))
            {
                if (args.UpdateData.FirstName.Length > 100)
                {
                    errors.Add(("firstName", "First name cannot exceed 100 characters."));
                }
            }

            // Validate last name (if provided)
            if (!string.IsNullOrWhiteSpace(args.UpdateData.LastName))
            {
                if (args.UpdateData.LastName.Length > 100)
                {
                    errors.Add(("lastName", "Last name cannot exceed 100 characters."));
                }
            }

            // Validate email (if provided)
            if (args.UpdateData.Email != null && !string.IsNullOrWhiteSpace(args.UpdateData.Email))
            {
                if (args.UpdateData.Email.Length > 100)
                {
                    errors.Add(("email", "Email cannot exceed 100 characters."));
                }
                else if (!IsValidEmail(args.UpdateData.Email))
                {
                    errors.Add(("email", "Email is not a valid email address."));
                }
            }

            // Validate phone number (if provided)
            if (args.UpdateData.PhoneNumber != null && !string.IsNullOrWhiteSpace(args.UpdateData.PhoneNumber))
            {
                if (args.UpdateData.PhoneNumber.Length > 20)
                {
                    errors.Add(("phoneNumber", "Phone number cannot exceed 20 characters."));
                }
            }

            // Validate location (if provided)
            if (args.UpdateData.Location != null && !string.IsNullOrWhiteSpace(args.UpdateData.Location))
            {
                if (args.UpdateData.Location.Length > 100)
                {
                    errors.Add(("location", "Location cannot exceed 100 characters."));
                }
            }

            await Task.CompletedTask;
            return errors;
        }

        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            // Fetch the person
            var person = await unitOfWork.Persons.GetByIdAsync(args.PersonId, track: true, cancellationToken);
            if (person == null)
            {
                throw new InvalidOperationException($"Person with ID {args.PersonId} not found.");
            }

            // Apply changes
            args.UpdateData.ApplyTo(person);

            // Save changes
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Return the updated person
            return new Result(new PersonDto().Map(person));
        }

        private static bool IsValidEmail(string email)
        {
            // Basic email validation using regex
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
    }
}

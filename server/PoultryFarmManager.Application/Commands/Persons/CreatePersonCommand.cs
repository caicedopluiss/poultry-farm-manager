using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Commands.Persons;

public sealed class CreatePersonCommand
{
    public record Args(NewPersonDto NewPerson);
    public record Result(PersonDto CreatedPerson);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var person = args.NewPerson.Map();

            var createdPerson = await unitOfWork.Persons.CreateAsync(person, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var personDto = new PersonDto().Map(createdPerson);
            var result = new Result(personDto);

            return result;
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            // Validate FirstName
            if (string.IsNullOrWhiteSpace(args.NewPerson.FirstName))
            {
                errors.Add(("firstName", "First name is required."));
            }
            else if (args.NewPerson.FirstName.Length > 50)
            {
                errors.Add(("firstName", "First name cannot exceed 50 characters."));
            }

            // Validate LastName
            if (string.IsNullOrWhiteSpace(args.NewPerson.LastName))
            {
                errors.Add(("lastName", "Last name is required."));
            }
            else if (args.NewPerson.LastName.Length > 50)
            {
                errors.Add(("lastName", "Last name cannot exceed 50 characters."));
            }

            // Validate Email (optional but must be valid if provided)
            if (!string.IsNullOrWhiteSpace(args.NewPerson.Email))
            {
                if (args.NewPerson.Email.Length > 100)
                {
                    errors.Add(("email", "Email cannot exceed 100 characters."));
                }
                else if (!IsValidEmail(args.NewPerson.Email))
                {
                    errors.Add(("email", "Email is not a valid email address."));
                }
            }

            // Validate PhoneNumber (optional but validate length if provided)
            if (!string.IsNullOrWhiteSpace(args.NewPerson.PhoneNumber))
            {
                if (args.NewPerson.PhoneNumber.Length > 20)
                {
                    errors.Add(("phoneNumber", "Phone number cannot exceed 20 characters."));
                }
            }

            // Validate Location (optional but validate length if provided)
            if (!string.IsNullOrWhiteSpace(args.NewPerson.Location))
            {
                if (args.NewPerson.Location.Length > 100)
                {
                    errors.Add(("location", "Location cannot exceed 100 characters."));
                }
            }

            await Task.CompletedTask;
            return errors;
        }

        private static bool IsValidEmail(string email)
        {
            // Basic email validation using regex
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }
    }
}

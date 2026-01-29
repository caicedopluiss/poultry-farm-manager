using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Commands.Vendors;

public sealed class CreateVendorCommand
{
    public record Args(NewVendorDto NewVendor);
    public record Result(VendorDto CreatedVendor);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var vendor = args.NewVendor.Map();

            var createdVendor = await unitOfWork.Vendors.CreateAsync(vendor, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload vendor with ContactPerson included
            var vendorWithContactPerson = await unitOfWork.Vendors.GetByIdAsync(createdVendor.Id, false, cancellationToken);

            var vendorDto = new VendorDto().Map(vendorWithContactPerson!);
            var result = new Result(vendorDto);

            return result;
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            // Validate Name
            if (string.IsNullOrWhiteSpace(args.NewVendor.Name))
            {
                errors.Add(("name", "Vendor name is required."));
            }
            else if (args.NewVendor.Name.Length > 100)
            {
                errors.Add(("name", "Vendor name cannot exceed 100 characters."));
            }

            // Validate Location (optional but validate length if provided)
            if (!string.IsNullOrWhiteSpace(args.NewVendor.Location))
            {
                if (args.NewVendor.Location.Length > 100)
                {
                    errors.Add(("location", "Location cannot exceed 100 characters."));
                }
            }

            // Validate ContactPersonId
            if (args.NewVendor.ContactPersonId == Guid.Empty)
            {
                errors.Add(("contactPersonId", "Contact person is required."));
            }
            else
            {
                // Check if contact person exists
                var contactPerson = await unitOfWork.Persons.GetByIdAsync(args.NewVendor.ContactPersonId, false, cancellationToken);
                if (contactPerson == null)
                {
                    errors.Add(("contactPersonId", "Contact person does not exist."));
                }
            }

            return errors;
        }
    }
}

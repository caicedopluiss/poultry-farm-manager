using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Commands.Vendors;

public sealed class UpdateVendorCommand
{
    public record Args(Guid Id, UpdateVendorDto UpdateVendor);
    public record Result(VendorDto UpdatedVendor);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var vendor = await unitOfWork.Vendors.GetByIdAsync(args.Id, track: true, cancellationToken)
                ?? throw new InvalidOperationException($"Vendor with id {args.Id} not found");

            args.UpdateVendor.ApplyTo(vendor);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload vendor with ContactPerson included
            var vendorWithContactPerson = await unitOfWork.Vendors.GetByIdAsync(vendor.Id, track: false, cancellationToken);

            var vendorDto = new VendorDto().Map(vendorWithContactPerson!);
            var result = new Result(vendorDto);

            return result;
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            // Validate Name (optional but validate if provided)
            if (args.UpdateVendor.Name is not null)
            {
                if (string.IsNullOrWhiteSpace(args.UpdateVendor.Name))
                {
                    errors.Add(("name", "Vendor name cannot be empty."));
                }
                else if (args.UpdateVendor.Name.Length > 100)
                {
                    errors.Add(("name", "Vendor name cannot exceed 100 characters."));
                }
            }

            // Validate Location (optional but validate length if provided)
            if (args.UpdateVendor.Location is not null && !string.IsNullOrWhiteSpace(args.UpdateVendor.Location))
            {
                if (args.UpdateVendor.Location.Length > 100)
                {
                    errors.Add(("location", "Location cannot exceed 100 characters."));
                }
            }

            // Validate ContactPersonId (optional but validate if provided)
            if (args.UpdateVendor.ContactPersonId is not null)
            {
                if (args.UpdateVendor.ContactPersonId == Guid.Empty)
                {
                    errors.Add(("contactPersonId", "Contact person cannot be empty."));
                }
                else
                {
                    // Check if contact person exists
                    var contactPerson = await unitOfWork.Persons.GetByIdAsync(args.UpdateVendor.ContactPersonId.Value, false, cancellationToken);
                    if (contactPerson == null)
                    {
                        errors.Add(("contactPersonId", "Contact person does not exist."));
                    }
                }
            }

            return errors;
        }
    }
}

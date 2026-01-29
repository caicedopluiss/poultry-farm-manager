using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.Vendors;

public sealed class GetVendorByIdQuery
{
    public record Args(Guid Id);
    public record Result(VendorDto? Vendor);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var vendor = await unitOfWork.Vendors.GetByIdAsync(args.Id, track: false, cancellationToken);
            if (vendor == null)
            {
                return new Result(null);
            }

            var vendorDto = new VendorDto().Map(vendor);
            return new Result(vendorDto);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<(string field, string error)>());
        }
    }
}

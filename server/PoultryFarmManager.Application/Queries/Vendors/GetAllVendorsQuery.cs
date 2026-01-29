using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.Vendors;

public sealed class GetAllVendorsQuery
{
    public record Args();
    public record Result(IEnumerable<VendorDto> Vendors);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var vendors = await unitOfWork.Vendors.GetAllAsync(cancellationToken);
            var vendorDtos = vendors.Select(v => new VendorDto().Map(v)).ToList();
            return new Result(vendorDtos);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<(string field, string error)>());
        }
    }
}

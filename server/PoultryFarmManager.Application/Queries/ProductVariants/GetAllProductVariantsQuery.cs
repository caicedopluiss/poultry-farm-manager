using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.ProductVariants;

public sealed class GetAllProductVariantsQuery
{
    public record Args();
    public record Result(IReadOnlyCollection<ProductVariantDto> ProductVariants);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var productVariants = await unitOfWork.ProductVariants.GetAllAsync(cancellationToken);

            var productVariantDtos = productVariants.Select(pv => new ProductVariantDto().Map(pv)).ToList();

            return new Result(productVariantDtos);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<(string field, string error)>());
        }
    }
}

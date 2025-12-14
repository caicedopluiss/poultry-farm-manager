using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.ProductVariants;

public sealed class GetProductVariantByIdQuery
{
    public record Args(Guid ProductVariantId);
    public record Result(ProductVariantDto? ProductVariant);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var productVariant = await unitOfWork.ProductVariants.GetByIdAsync(args.ProductVariantId, cancellationToken: cancellationToken);

            if (productVariant == null) return new Result(null);

            var productVariantDto = new ProductVariantDto().Map(productVariant);

            return new Result(productVariantDto);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<(string field, string error)>());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.Products;

public sealed class GetProductByIdQuery
{
    public record Args(Guid ProductId);
    public record Result(ProductDto? Product);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var product = await unitOfWork.Products.GetByIdAsync(args.ProductId, cancellationToken: cancellationToken);

            if (product == null) return new Result(null);

            var productDto = new ProductDto().Map(product);

            return new Result(productDto);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<(string field, string error)>());
        }
    }
}

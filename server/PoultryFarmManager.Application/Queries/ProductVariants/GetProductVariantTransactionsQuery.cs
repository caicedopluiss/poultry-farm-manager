using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.ProductVariants;

public static class GetProductVariantTransactionsQuery
{
    public record Args(Guid ProductVariantId);
    public record Result(IEnumerable<TransactionDto> Transactions);

    public class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var transactions = await unitOfWork.Transactions.GetByProductVariantIdAsync(args.ProductVariantId, cancellationToken);

            var transactionDtos = transactions
                .Select(t => new TransactionDto().Map(t))
                .OrderByDescending(t => t.Date)
                .ToList();

            return new Result(transactionDtos);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (args.ProductVariantId == Guid.Empty)
            {
                errors.Add(("productVariantId", "Product variant ID is required."));
            }

            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.Assets;

public static class GetAssetTransactionsQuery
{
    public record Args(Guid AssetId);
    public record Result(IEnumerable<TransactionDto> Transactions);

    public class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var transactions = await unitOfWork.Transactions.GetByAssetIdAsync(args.AssetId, cancellationToken);

            var transactionDtos = transactions
                .Select(t => new TransactionDto().Map(t))
                .OrderByDescending(t => t.Date)
                .ToList();

            return new Result(transactionDtos);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (args.AssetId == Guid.Empty)
            {
                errors.Add(("assetId", "Asset ID is required."));
            }

            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }
    }
}

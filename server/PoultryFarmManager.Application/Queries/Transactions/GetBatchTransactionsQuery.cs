using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.Transactions;

public class GetBatchTransactionsQuery
{
    public record Args(Guid BatchId);
    public record Result(IEnumerable<TransactionDto> Transactions);

    public class Handler(ITransactionsRepository transactionsRepository) : AppRequestHandler<Args, Result>
    {
        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (args.BatchId == Guid.Empty)
            {
                errors.Add(("batchId", "Batch ID is required."));
            }

            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }

        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var batchTransactions = await transactionsRepository.GetByBatchIdAsync(args.BatchId, cancellationToken);

            var mapper = new TransactionDto();
            var transactionDtos = batchTransactions
                .Select(t => mapper.Map(t))
                .ToList();

            return new Result(transactionDtos);
        }
    }
}

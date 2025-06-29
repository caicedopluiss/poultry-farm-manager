using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Operations.DTOs;
using PoultryFarmManager.Application.Operations.Repositories;
using PoultryFarmManager.Core.Operations.Models;
using SharedLib.CQRS;

namespace PoultryFarmManager.Application.Operations.Commands;

public sealed class CreateBroilerBatchCommand
{
    public record Args(NewBroilerBatchDto Payload);
    public record Result(BroilerBatchDto BatchDto);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var batch = args.Payload.ToCoreModel();
            batch.CurrentPopulation = args.Payload.InitialPopulation;

            var addedBatch = await unitOfWork.BroilerBatches.AddAsync(batch, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            var result = BroilerBatchDto.FromCore(addedBatch);

            return new(result);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (string.IsNullOrWhiteSpace(args.Payload.BatchName))
            {
                errors.Add(("batchName", "Batch name cannot be empty."));
            }

            if (args.Payload.InitialPopulation <= 0)
            {
                errors.Add(("initialPopulation", "Initial population must be greater than zero."));
            }

            if (!string.IsNullOrEmpty(args.Payload.StartClientDate) && !Utils.IsIso8601DateStringValid(args.Payload.StartClientDate))
            {
                errors.Add(("startClientDate", "Invalid ISO 8601 date format."));
            }

            if (!Enum.TryParse<BroilerBatchStatus>(args.Payload.Status, out var status) || !Enum.IsDefined(typeof(BroilerBatchStatus), status))
            {
                errors.Add(("status", "Invalid status value."));
            }

            if (args.Payload.Notes?.Length > 500)
            {
                errors.Add(("notes", "Notes cannot exceed 500 characters."));
            }

            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }
    }
}
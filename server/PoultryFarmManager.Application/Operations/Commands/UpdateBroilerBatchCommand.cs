using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Operations.DTOs;
using PoultryFarmManager.Core.Operations.Models;
using SharedLib.CQRS;

namespace PoultryFarmManager.Application.Operations.Commands;

public sealed class UpdateBroilerBatchCommand
{
    public record Args(Guid Id, UpdateBroilerBatchDto Payload);
    public record Result(BroilerBatchDto BatchDto);


    public class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        BroilerBatch updatingBatch = null!;

        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            _ = args.Payload.ToCoreModel(updatingBatch);
            await unitOfWork.BroilerBatches.UpdateAsync(updatingBatch, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedBatch = await unitOfWork.BroilerBatches.GetByIdAsync(args.Id, includeFinancialTransaction: true, cancellationToken: cancellationToken);

            var result = BroilerBatchDto.FromCore(updatedBatch!);

            return new(result);
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (args.Id == Guid.Empty)
            {
                errors.Add(("id", "Batch ID cannot be empty."));
            }
            else
            {
                var updatingEntity = await unitOfWork.BroilerBatches.GetByIdAsync(args.Id, cancellationToken: cancellationToken);
                if (updatingEntity is null)
                {
                    errors.Add(("id", "Batch not found."));
                    return errors;
                }
                else
                {
                    updatingBatch = updatingEntity;
                }
            }

            if (string.IsNullOrWhiteSpace(args.Payload.BatchName))
            {
                errors.Add(("batchName", "Batch name cannot be empty."));
            }

            if (args.Payload.InitialPopulation <= 0)
            {
                errors.Add(("initialPopulation", "Initial population must be greater than zero."));
            }

            if (!string.IsNullOrEmpty(args.Payload.StartClientDate?.Trim()) && !Utils.IsIso8601DateStringValid(args.Payload.StartClientDate))
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

            if (!string.IsNullOrEmpty(args.Payload.ProcessingStartClientDate?.Trim()) &&
               !Utils.IsIso8601DateStringValid(args.Payload.ProcessingStartClientDate))
            {
                errors.Add(("processingStartClientDate", "Invalid ISO 8601 date format for processing start date."));
            }

            if (!string.IsNullOrEmpty(args.Payload.ProcessingEndClientDate) &&
               !Utils.IsIso8601DateStringValid(args.Payload.ProcessingEndClientDate))
            {
                errors.Add(("processingEndClientDate", "Invalid ISO 8601 date format for processing end date."));
            }

            return errors;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.Finances.DTOs;
using PoultryFarmManager.Application.Operations.DTOs;
using PoultryFarmManager.Core.Finances;
using PoultryFarmManager.Core.Finances.Models;
using PoultryFarmManager.Core.Operations.Models;
using SharedLib.CQRS;

namespace PoultryFarmManager.Application.Operations.Commands;

public sealed class CreateBroilerBatchCommand
{
    public record Args(NewBroilerBatchDto BatchInfo, NewFinancialTransactionDto FinancialTransactionInfo);
    public record Result(BroilerBatchDto BatchDto);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            // Create financial transaction
            FinancialTransaction financialTransaction = args.FinancialTransactionInfo.ToCoreModel();
            financialTransaction = await unitOfWork.Finances.AddFinancialTransactionAsync(financialTransaction, cancellationToken);

            // Create broiler batch
            var batch = args.BatchInfo.ToCoreModel();
            batch.CurrentPopulation = args.BatchInfo.InitialPopulation;

            // Set the financial transaction for the batch
            batch.FinancialTransaction = financialTransaction;

            var addedBatch = await unitOfWork.BroilerBatches.AddAsync(batch, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            var result = BroilerBatchDto.FromCore(addedBatch);

            return new(result);
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            // Validate batch info
            if (string.IsNullOrWhiteSpace(args.BatchInfo.BatchName))
            {
                errors.Add(("batchInfo.batchName", "Batch name cannot be empty."));
            }

            if (args.BatchInfo.InitialPopulation <= 0)
            {
                errors.Add(("batchInfo.initialPopulation", "Initial population must be greater than zero."));
            }

            if (!string.IsNullOrWhiteSpace(args.BatchInfo.StartClientDate) && !Utils.IsIso8601DateStringValid(args.BatchInfo.StartClientDate))
            {
                errors.Add(("batchInfo.startClientDate", "Invalid ISO 8601 date format."));
            }

            if (!Enum.TryParse<BroilerBatchStatus>(args.BatchInfo.Status, out var status) || !Enum.IsDefined(typeof(BroilerBatchStatus), status))
            {
                errors.Add(("batchInfo.status", "Invalid status value."));
            }

            if (args.BatchInfo.Notes?.Length > 500)
            {
                errors.Add(("batchInfo.notes", "Notes cannot exceed 500 characters."));
            }

            // Validate financial transaction info
            if (args.FinancialTransactionInfo.Amount <= 0)
            {
                errors.Add(("financialTransactionInfo.amount", "Financial transaction amount must be greater than zero."));
            }

            if (args.FinancialTransactionInfo.PaidAmount.HasValue && args.FinancialTransactionInfo.PaidAmount.Value < 0)
            {
                errors.Add(("financialTransactionInfo.paidAmount", "Paid amount cannot be negative."));
            }

            if (args.FinancialTransactionInfo.PaidAmount.HasValue && args.FinancialTransactionInfo.PaidAmount.Value > args.FinancialTransactionInfo.Amount)
            {
                errors.Add(("financialTransactionInfo.paidAmount", "Paid amount cannot exceed the total amount."));
            }

            if (!Enum.TryParse<FinancialTransactionType>(args.FinancialTransactionInfo.Type, out var transactionType) || !Enum.IsDefined(typeof(FinancialTransactionType), transactionType))
            {
                errors.Add(("financialTransactionInfo.type", "Invalid financial transaction type."));
            }

            if (!Enum.TryParse<PaymentStatus>(args.FinancialTransactionInfo.Status, out var paymentStatus) || !Enum.IsDefined(typeof(PaymentStatus), paymentStatus))
            {
                errors.Add(("financialTransactionInfo.status", "Invalid financial transaction status."));
            }

            if (!Enum.TryParse<FinancialTransactionCategory>(args.FinancialTransactionInfo.Category, out var transactionCategory) || !Enum.IsDefined(typeof(FinancialTransactionCategory), transactionCategory))
            {
                errors.Add(("financialTransactionInfo.category", "Invalid financial transaction category."));
            }

            if (!string.IsNullOrWhiteSpace(args.FinancialTransactionInfo.DueClientDate) && !Utils.IsIso8601DateStringValid(args.FinancialTransactionInfo.DueClientDate))
            {
                errors.Add(("financialTransactionInfo.dueClientDate", "Invalid ISO 8601 date format."));
            }

            if (args.FinancialTransactionInfo.Notes?.Length > 500)
            {
                errors.Add(("financialTransactionInfo.notes", "Notes cannot exceed 500 characters."));
            }

            if (!args.FinancialTransactionInfo.FinancialEntityId.HasValue && args.FinancialTransactionInfo.FinancialEntityInfo is null)
            {
                errors.Add(("financialTransactionInfo.financialEntityId", "Financial entity ID or financial entity info must be provided."));
            }

            if (args.FinancialTransactionInfo.FinancialEntityId.HasValue && args.FinancialTransactionInfo.FinancialEntityInfo is not null)
            {
                errors.Add(("financialTransactionInfo.financialEntityId", "Cannot provide both financial entity ID and financial entity info."));
            }

            if (args.FinancialTransactionInfo.FinancialEntityId.HasValue && args.FinancialTransactionInfo.FinancialEntityInfo is null)
            {
                if (args.FinancialTransactionInfo.FinancialEntityId.Value == Guid.Empty)
                {
                    errors.Add(("financialTransactionInfo.financialEntityId", "Financial entity ID cannot be empty."));
                }
                else
                {
                    var financialEntity = await unitOfWork.Finances.GetFinancialEntityByIdAsync(args.FinancialTransactionInfo.FinancialEntityId.Value, false, cancellationToken);
                    if (financialEntity is null)
                        errors.Add(("financialTransactionInfo.financialEntityId", "Financial entity with the provided ID does not exist."));
                }
            }

            // Validate financial entity info if provided
            if (args.FinancialTransactionInfo.FinancialEntityInfo is not null)
            {
                if (string.IsNullOrWhiteSpace(args.FinancialTransactionInfo.FinancialEntityInfo.Name))
                {
                    errors.Add(("financialTransactionInfo.financialEntityInfo.name", "Financial entity name cannot be empty."));
                }
                else if (args.FinancialTransactionInfo.FinancialEntityInfo.Name.Length > 100)
                {
                    errors.Add(("financialTransactionInfo.financialEntityInfo.name", "Financial entity name cannot exceed 100 characters."));
                }

                if (!string.IsNullOrWhiteSpace(args.FinancialTransactionInfo.FinancialEntityInfo.ContactPhoneNumber) && args.FinancialTransactionInfo.FinancialEntityInfo.ContactPhoneNumber.Length > 15)
                {
                    errors.Add(("financialTransactionInfo.financialEntityInfo.contactPhoneNumber", "Financial entity contact phone number cannot exceed 15 characters."));
                }

                if (!Enum.TryParse<FinancialEntityType>(args.FinancialTransactionInfo.FinancialEntityInfo.Type, out var financialEntityType) || !Enum.IsDefined(typeof(FinancialEntityType), financialEntityType))
                {
                    errors.Add(("financialTransactionInfo.financialEntityInfo.type", "Invalid financial entity type."));
                }
            }

            return errors;
        }
    }
}
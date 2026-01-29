using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Application.Commands.Transactions;

public sealed class CreateTransactionCommand
{
    public record Args(NewTransactionDto NewTransaction);
    public record Result(TransactionDto CreatedTransaction);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var transaction = args.NewTransaction.Map();

            var createdTransaction = await unitOfWork.Transactions.CreateAsync(transaction, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with navigation properties
            var transactionWithNavigation = await unitOfWork.Transactions.GetByIdAsync(createdTransaction.Id, track: false, cancellationToken);

            var transactionDto = new TransactionDto().Map(transactionWithNavigation!);
            var result = new Result(transactionDto);

            return result;
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            // Validate Title
            if (string.IsNullOrWhiteSpace(args.NewTransaction.Title))
            {
                errors.Add(("title", "Title is required."));
            }
            else if (args.NewTransaction.Title.Length > 100)
            {
                errors.Add(("title", "Title cannot exceed 100 characters."));
            }

            // Validate Date
            if (!Utils.IsIso8601DateStringValid(args.NewTransaction.DateClientIsoString))
            {
                errors.Add(("dateClientIsoString", "Date is not a valid ISO 8601 date string."));
            }

            // Validate Type
            if (string.IsNullOrWhiteSpace(args.NewTransaction.Type))
            {
                errors.Add(("type", "Transaction type is required."));
            }
            else if (!Enum.TryParse<TransactionType>(args.NewTransaction.Type, ignoreCase: true, out _))
            {
                errors.Add(("type", $"Invalid transaction type: '{args.NewTransaction.Type}'. Valid types are: Income, Expense."));
            }

            // Validate UnitPrice
            if (args.NewTransaction.UnitPrice <= 0)
            {
                errors.Add(("unitPrice", "Unit price must be greater than zero."));
            }

            // Validate Quantity
            if (args.NewTransaction.Quantity.HasValue && args.NewTransaction.Quantity.Value <= 0)
            {
                errors.Add(("quantity", "Quantity must be greater than zero if provided."));
            }

            // Validate TransactionAmount
            if (args.NewTransaction.TransactionAmount <= 0)
            {
                errors.Add(("transactionAmount", "Transaction amount must be greater than zero."));
            }

            // Validate Notes
            if (!string.IsNullOrWhiteSpace(args.NewTransaction.Notes) && args.NewTransaction.Notes.Length > 500)
            {
                errors.Add(("notes", "Notes cannot exceed 500 characters."));
            }

            // Validate ProductVariantId existence
            if (args.NewTransaction.ProductVariantId.HasValue && args.NewTransaction.ProductVariantId.Value != Guid.Empty)
            {
                var productVariant = await unitOfWork.ProductVariants.GetByIdAsync(args.NewTransaction.ProductVariantId.Value, track: false, cancellationToken);
                if (productVariant == null)
                {
                    errors.Add(("productVariantId", $"Product variant with ID '{args.NewTransaction.ProductVariantId.Value}' not found."));
                }
            }

            // Validate BatchId existence
            if (args.NewTransaction.BatchId.HasValue && args.NewTransaction.BatchId.Value != Guid.Empty)
            {
                var batch = await unitOfWork.Batches.GetByIdAsync(args.NewTransaction.BatchId.Value, track: false, cancellationToken);
                if (batch == null)
                {
                    errors.Add(("batchId", $"Batch with ID '{args.NewTransaction.BatchId.Value}' not found."));
                }
            }

            // Validate VendorId existence
            if (args.NewTransaction.VendorId.HasValue && args.NewTransaction.VendorId.Value != Guid.Empty)
            {
                var vendor = await unitOfWork.Vendors.GetByIdAsync(args.NewTransaction.VendorId.Value, track: false, cancellationToken);
                if (vendor == null)
                {
                    errors.Add(("vendorId", $"Vendor with ID '{args.NewTransaction.VendorId.Value}' not found."));
                }
            }

            // Validate CustomerId existence
            if (args.NewTransaction.CustomerId.HasValue && args.NewTransaction.CustomerId.Value != Guid.Empty)
            {
                var customer = await unitOfWork.Persons.GetByIdAsync(args.NewTransaction.CustomerId.Value, track: false, cancellationToken);
                if (customer == null)
                {
                    errors.Add(("customerId", $"Customer with ID '{args.NewTransaction.CustomerId.Value}' not found."));
                }
            }

            // Business rule: If ProductVariantId has a value, transaction must be an expense and cannot have CustomerId
            if (args.NewTransaction.ProductVariantId.HasValue && args.NewTransaction.ProductVariantId.Value != Guid.Empty)
            {
                if (Enum.TryParse<TransactionType>(args.NewTransaction.Type, ignoreCase: true, out var transactionType))
                {
                    if (transactionType != TransactionType.Expense)
                    {
                        errors.Add(("productVariantId", "Product variant can only be assigned to expense transactions."));
                    }
                }

                if (args.NewTransaction.CustomerId.HasValue && args.NewTransaction.CustomerId.Value != Guid.Empty)
                {
                    errors.Add(("customerId", "Customer cannot be specified when product variant is assigned."));
                }
            }

            // Business rule: CustomerId must not be null for income transactions
            if (Enum.TryParse<TransactionType>(args.NewTransaction.Type, ignoreCase: true, out var type))
            {
                if (type == TransactionType.Income)
                {
                    if (!args.NewTransaction.CustomerId.HasValue || args.NewTransaction.CustomerId.Value == Guid.Empty)
                    {
                        errors.Add(("customerId", "Customer is required for income transactions."));
                    }
                }
            }

            return errors;
        }
    }
}

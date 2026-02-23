using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Application.Commands.ProductVariants;

public sealed class CreateProductVariantCommand
{
    public record Args(NewProductVariantDto NewProductVariant);
    public record Result(ProductVariantDto CreatedProductVariant);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var product = await unitOfWork.Products.GetByIdAsync(args.NewProductVariant.ProductId, track: true, cancellationToken: cancellationToken) ??
                throw new InvalidOperationException($"Product with ID {args.NewProductVariant.ProductId} not found.");

            var productVariant = args.NewProductVariant.Map();

            // Calculate the variant's total stock: Stock + (Stock * Quantity)
            var totalVariantStock = productVariant.Stock + (productVariant.Stock * productVariant.Quantity);
            productVariant.Stock = totalVariantStock;

            var createdProductVariant = await unitOfWork.ProductVariants.CreateAsync(productVariant, cancellationToken);

            // Create a transaction for the product variant purchase
            var transaction = new Transaction
            {
                Title = $"Purchase of product variant: {createdProductVariant.Name}",
                Date = DateTime.UtcNow,
                Type = TransactionType.Expense,
                UnitPrice = args.NewProductVariant.UnitPrice,
                Quantity = productVariant.Quantity,
                TransactionAmount = args.NewProductVariant.UnitPrice * productVariant.Quantity,
                ProductVariantId = createdProductVariant.Id,
                VendorId = args.NewProductVariant.VendorId,
                BatchId = null,
                CustomerId = null,
                Notes = "Automatically created during product variant registration"
            };

            await unitOfWork.Transactions.CreateAsync(transaction, cancellationToken);

            // Update the product to ensure the stock change is tracked
            await unitOfWork.Products.UpdateAsync(product, cancellationToken);

            // Save product variant and transaction atomically in a single database transaction
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var productVariantDto = new ProductVariantDto().Map(createdProductVariant);
            var result = new Result(productVariantDto);

            return result;
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (string.IsNullOrWhiteSpace(args.NewProductVariant.Name))
            {
                errors.Add(("name", "Name is required."));
            }
            else if (args.NewProductVariant.Name.Length > 100)
            {
                errors.Add(("name", "Name cannot exceed 100 characters."));
            }

            if (!string.IsNullOrWhiteSpace(args.NewProductVariant.Description) && args.NewProductVariant.Description.Length > 500)
            {
                errors.Add(("description", "Description cannot exceed 500 characters."));
            }

            if (args.NewProductVariant.Stock < 0)
            {
                errors.Add(("stock", "Stock cannot be negative."));
            }

            if (args.NewProductVariant.Quantity <= 0)
            {
                errors.Add(("quantity", "Quantity must be greater than zero."));
            }

            if (string.IsNullOrWhiteSpace(args.NewProductVariant.UnitOfMeasure))
            {
                errors.Add(("unitOfMeasure", "Unit of measure is required."));
            }
            else if (!Enum.TryParse<UnitOfMeasure>(args.NewProductVariant.UnitOfMeasure, ignoreCase: true, out _))
            {
                errors.Add(("unitOfMeasure", "Invalid unit of measure."));
            }

            if (args.NewProductVariant.VendorId == Guid.Empty)
            {
                errors.Add(("vendorId", "Vendor is required."));
            }
            else
            {
                var vendor = await unitOfWork.Vendors.GetByIdAsync(args.NewProductVariant.VendorId, cancellationToken: cancellationToken);
                if (vendor == null)
                {
                    errors.Add(("vendorId", "Vendor not found."));
                }
            }

            if (args.NewProductVariant.UnitPrice <= 0)
            {
                errors.Add(("unitPrice", "Unit price must be greater than zero."));
            }

            return errors;
        }
    }
}

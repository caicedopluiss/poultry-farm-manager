using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;

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

            // Update product stock by adding the variant's total quantity
            // If the variant has a different unit of measure, convert it first
            if (productVariant.UnitOfMeasure.TryConvert(product.UnitOfMeasure, totalVariantStock, out var convertedStock))
            {
                product.Stock += convertedStock;
            }
            else
            {
                throw new InvalidOperationException($"Cannot convert quantity from {productVariant.UnitOfMeasure} to {product.UnitOfMeasure}.");
            }

            var createdProductVariant = await unitOfWork.ProductVariants.CreateAsync(productVariant, cancellationToken);
            // // Update the product to ensure the stock change is tracked
            await unitOfWork.Products.UpdateAsync(product, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var productVariantDto = new ProductVariantDto().Map(createdProductVariant);
            var result = new Result(productVariantDto);

            return result;
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
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

            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }
    }
}

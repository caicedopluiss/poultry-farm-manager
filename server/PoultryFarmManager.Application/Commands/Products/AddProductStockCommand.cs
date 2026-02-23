using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Commands.Products;

public sealed class AddProductStockCommand
{
    public record Args(Guid ProductId, Guid ProductVariantId, int Quantity);
    public record Result(ProductDto UpdatedProduct);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var product = await unitOfWork.Products.GetByIdAsync(args.ProductId, track: true, cancellationToken: cancellationToken) ??
                throw new InvalidOperationException($"Product with ID {args.ProductId} not found.");

            var productVariant = await unitOfWork.ProductVariants.GetByIdAsync(args.ProductVariantId, track: true, cancellationToken: cancellationToken) ??
                throw new InvalidOperationException($"Product variant with ID {args.ProductVariantId} not found.");

            // Verify the variant belongs to the product
            if (productVariant.ProductId != args.ProductId)
            {
                throw new InvalidOperationException($"Product variant does not belong to the specified product.");
            }

            // Calculate the stock to add based on the variant's unit of measure
            var stockToAdd = productVariant.Stock * args.Quantity;

            // Update variant stock
            productVariant.Stock += stockToAdd;

            // Update product stock - convert units if needed
            if (productVariant.UnitOfMeasure == product.UnitOfMeasure)
            {
                product.Stock += stockToAdd;
            }
            else if (productVariant.UnitOfMeasure.TryConvert(product.UnitOfMeasure, stockToAdd, out var convertedStock))
            {
                product.Stock += convertedStock;
            }
            // If units are incompatible, only update variant stock

            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload product with navigation properties
            var updatedProduct = await unitOfWork.Products.GetByIdAsync(product.Id, track: false, cancellationToken);
            var productDto = new ProductDto().Map(updatedProduct!);

            return new Result(productDto);
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (args.ProductId == Guid.Empty)
            {
                errors.Add(("productId", "Product ID is required."));
            }

            if (args.ProductVariantId == Guid.Empty)
            {
                errors.Add(("productVariantId", "Product variant ID is required."));
            }

            if (args.Quantity <= 0)
            {
                errors.Add(("quantity", "Quantity must be greater than 0."));
            }

            return await Task.FromResult(errors);
        }
    }
}

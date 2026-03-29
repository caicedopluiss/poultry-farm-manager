using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Application.Commands.Products;

public sealed class AddProductStockCommand
{
    public record Args(Guid ProductId, Guid ProductVariantId, int Quantity);
    public record Result(ProductDto UpdatedProduct);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        private Product? _product;
        private ProductVariant? _productVariant;

        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            // Re-fetch tracked instance for the product (variant is read-only, reuse cached copy)
            _product = await unitOfWork.Products.GetByIdAsync(args.ProductId, track: true, cancellationToken: cancellationToken);

            // Calculate the stock to add based on the variant's unit-of-measure and package size
            var stockToAdd = _productVariant!.Stock * args.Quantity;

            // Update product stock - convert units if needed
            if (_productVariant.UnitOfMeasure == _product!.UnitOfMeasure)
            {
                _product.Stock += stockToAdd;
            }
            else if (_productVariant.UnitOfMeasure.TryConvert(_product.UnitOfMeasure, stockToAdd, out var convertedStock))
            {
                _product.Stock += convertedStock;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload product with navigation properties
            var updatedProduct = await unitOfWork.Products.GetByIdAsync(_product.Id, track: false, cancellationToken);
            var productDto = new ProductDto().Map(updatedProduct!);

            return new Result(productDto);
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (args.ProductId == Guid.Empty)
            {
                errors.Add(("productId", "Product ID is required."));
                return errors;
            }

            if (args.ProductVariantId == Guid.Empty)
            {
                errors.Add(("productVariantId", "Product variant ID is required."));
                return errors;
            }

            if (args.Quantity <= 0)
            {
                errors.Add(("quantity", "Quantity must be greater than 0."));
                return errors;
            }

            _product = await unitOfWork.Products.GetByIdAsync(args.ProductId, track: false, cancellationToken: cancellationToken);
            if (_product is null)
            {
                errors.Add(("productId", "Product not found."));
                return errors;
            }

            _productVariant = await unitOfWork.ProductVariants.GetByIdAsync(args.ProductVariantId, track: false, cancellationToken: cancellationToken);
            if (_productVariant is null)
            {
                errors.Add(("productVariantId", "Product variant not found."));
                return errors;
            }

            if (_productVariant.ProductId != args.ProductId)
                errors.Add(("productVariantId", "Product variant does not belong to the specified product."));

            if (_productVariant.UnitOfMeasure != _product.UnitOfMeasure &&
                !_productVariant.UnitOfMeasure.TryConvert(_product.UnitOfMeasure, 1m, out _))
            {
                errors.Add(("productVariantId", $"Variant unit of measure ({_productVariant.UnitOfMeasure}) is incompatible with product unit of measure ({_product.UnitOfMeasure})."));
            }

            return errors;
        }
    }
}

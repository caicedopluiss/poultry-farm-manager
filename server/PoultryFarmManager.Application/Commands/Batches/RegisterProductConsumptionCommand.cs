using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.Core.Models.BatchActivities;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Application.Commands.Batches;

public sealed class RegisterProductConsumptionCommand
{
    public record Args(Guid BatchId, NewProductConsumptionDto ProductConsumption);
    public record Result(ProductConsumptionActivityDto ProductConsumption);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        private Batch? batch;
        private Product? product;

        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            // Create product consumption activity
            var productConsumption = args.ProductConsumption.Map();
            productConsumption.BatchId = args.BatchId;

            var createdActivity = await unitOfWork.BatchActivities.CreateAsync(productConsumption, cancellationToken);

            // Parse the unit of measure from the activity
            var activityUnitOfMeasure = Enum.Parse<UnitOfMeasure>(args.ProductConsumption.UnitOfMeasure, ignoreCase: true);

            // Update product stock with unit conversion
            activityUnitOfMeasure.TryConvert(product!.UnitOfMeasure, args.ProductConsumption.Stock, out var convertedStock);
            product.Stock -= convertedStock;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var productConsumptionDto = ProductConsumptionActivityDto.MapFrom((ProductConsumptionBatchActivity)createdActivity);
            var result = new Result(productConsumptionDto);

            return result;
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (args.BatchId == Guid.Empty)
            {
                errors.Add(("batchId", "Batch ID is required."));
            }
            else
            {
                batch = await unitOfWork.Batches.GetByIdAsync(args.BatchId, track: true, cancellationToken);
                if (batch is null)
                {
                    throw new InvalidOperationException($"Batch with ID {args.BatchId} not found.");
                }
            }

            if (args.ProductConsumption.ProductId == Guid.Empty)
            {
                errors.Add(("productId", "Product ID is required."));
            }
            else
            {
                product = await unitOfWork.Products.GetByIdAsync(args.ProductConsumption.ProductId, true, cancellationToken);
                if (product is null)
                {
                    errors.Add(("productId", $"Product with ID {args.ProductConsumption.ProductId} not found."));
                }
            }

            if (args.ProductConsumption.Stock <= 0)
            {
                errors.Add(("stock", "Stock must be greater than zero."));
            }

            // Validate unit of measure
            if (string.IsNullOrWhiteSpace(args.ProductConsumption.UnitOfMeasure))
            {
                errors.Add(("unitOfMeasure", "Unit of measure is required."));
            }
            else if (!Enum.TryParse<UnitOfMeasure>(args.ProductConsumption.UnitOfMeasure, ignoreCase: true, out var unitOfMeasure))
            {
                errors.Add(("unitOfMeasure", $"Invalid unit of measure: '{args.ProductConsumption.UnitOfMeasure}'. Valid values are: Grams, Kilograms, Pounds, Liters, Milliliters, Units."));
            }
            else if (product is not null)
            {
                // Validate that conversion is possible and check available stock
                if (!unitOfMeasure.TryConvert(product.UnitOfMeasure, args.ProductConsumption.Stock, out var convertedStock))
                {
                    errors.Add(("unitOfMeasure", $"Cannot convert from {unitOfMeasure} to {product.UnitOfMeasure}. Units must be compatible."));
                }
                else if (convertedStock > product.Stock)
                {
                    errors.Add(("stock", $"Cannot consume {args.ProductConsumption.Stock} {unitOfMeasure}. Only {product.Stock} {product.UnitOfMeasure} available in stock (equivalent to {convertedStock} {product.UnitOfMeasure} requested)."));
                }
            }

            if (!Utils.IsIso8601DateStringValid(args.ProductConsumption.DateClientIsoString))
            {
                errors.Add(("dateClientIsoString", "Date is not a valid ISO 8601 date string."));
            }

            if (!string.IsNullOrWhiteSpace(args.ProductConsumption.Notes) && args.ProductConsumption.Notes.Length > 500)
            {
                errors.Add(("notes", "Notes cannot exceed 500 characters."));
            }

            return errors;
        }
    }
}

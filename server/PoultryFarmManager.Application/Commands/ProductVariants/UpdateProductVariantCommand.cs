using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Application.Commands.ProductVariants;

public sealed class UpdateProductVariantCommand
{
    public record Args(Guid ProductVariantId, UpdateProductVariantDto UpdateData);
    public record Result(ProductVariantDto UpdatedProductVariant);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var productVariant = await unitOfWork.ProductVariants.GetByIdAsync(args.ProductVariantId, track: true, cancellationToken: cancellationToken) ??
                throw new InvalidOperationException($"Product variant with ID {args.ProductVariantId} not found.");

            args.UpdateData.ApplyTo(productVariant);

            await unitOfWork.ProductVariants.UpdateAsync(productVariant, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var productVariantDto = new ProductVariantDto().Map(productVariant);
            var result = new Result(productVariantDto);

            return result;
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (args.UpdateData == null)
            {
                errors.Add(("updateData", "Update data is required."));
                return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
            }

            if (!string.IsNullOrWhiteSpace(args.UpdateData.Name) && args.UpdateData.Name.Length > 100)
            {
                errors.Add(("name", "Name cannot exceed 100 characters."));
            }

            if (!string.IsNullOrWhiteSpace(args.UpdateData.UnitOfMeasure) && !Enum.TryParse<UnitOfMeasure>(args.UpdateData.UnitOfMeasure, ignoreCase: true, out _))
            {
                errors.Add(("unitOfMeasure", "Invalid unit of measure."));
            }

            if (!string.IsNullOrWhiteSpace(args.UpdateData.Description) && args.UpdateData.Description.Length > 500)
            {
                errors.Add(("description", "Description cannot exceed 500 characters."));
            }

            if (args.UpdateData.Stock.HasValue && args.UpdateData.Stock.Value < 0)
            {
                errors.Add(("stock", "Stock cannot be negative."));
            }

            if (args.UpdateData.Quantity.HasValue && args.UpdateData.Quantity.Value <= 0)
            {
                errors.Add(("quantity", "Quantity must be greater than zero."));
            }

            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }
    }
}

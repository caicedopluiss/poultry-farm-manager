using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Application.Commands.Products;

public sealed class UpdateProductCommand
{
    public record Args(Guid ProductId, UpdateProductDto UpdateData);
    public record Result(ProductDto UpdatedProduct);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var product = await unitOfWork.Products.GetByIdAsync(args.ProductId, track: true, cancellationToken: cancellationToken) ??
                throw new InvalidOperationException($"Product with ID {args.ProductId} not found.");

            args.UpdateData.ApplyTo(product);

            await unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var productDto = new ProductDto().Map(product);
            var result = new Result(productDto);

            return result;
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (!string.IsNullOrWhiteSpace(args.UpdateData.Name) && args.UpdateData.Name.Length > 100)
            {
                errors.Add(("name", "Name cannot exceed 100 characters."));
            }

            if (!string.IsNullOrWhiteSpace(args.UpdateData.Manufacturer) && args.UpdateData.Manufacturer.Length > 100)
            {
                errors.Add(("manufacturer", "Manufacturer cannot exceed 100 characters."));
            }

            if (args.UpdateData.Stock.HasValue && args.UpdateData.Stock.Value < 0)
            {
                errors.Add(("stock", "Stock cannot be negative."));
            }

            if (!string.IsNullOrWhiteSpace(args.UpdateData.UnitOfMeasure) && !Enum.TryParse<UnitOfMeasure>(args.UpdateData.UnitOfMeasure, ignoreCase: true, out _))
            {
                errors.Add(("unitOfMeasure", "Unit of Measure is not valid."));
            }

            if (!string.IsNullOrWhiteSpace(args.UpdateData.Description) && args.UpdateData.Description.Length > 500)
            {
                errors.Add(("description", "Description cannot exceed 500 characters."));
            }

            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Application.Commands.Products;

public sealed class CreateProductCommand
{
    public record Args(NewProductDto NewProduct);
    public record Result(ProductDto CreatedProduct);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var product = args.NewProduct.Map();

            var createdProduct = await unitOfWork.Products.CreateAsync(product, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var productDto = new ProductDto().Map(createdProduct);
            var result = new Result(productDto);

            return result;
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (string.IsNullOrWhiteSpace(args.NewProduct.Name))
            {
                errors.Add(("name", "Name is required."));
            }
            else if (args.NewProduct.Name.Length > 100)
            {
                errors.Add(("name", "Name cannot exceed 100 characters."));
            }

            if (string.IsNullOrWhiteSpace(args.NewProduct.Manufacturer))
            {
                errors.Add(("manufacturer", "Manufacturer is required."));
            }
            else if (args.NewProduct.Manufacturer.Length > 100)
            {
                errors.Add(("manufacturer", "Manufacturer cannot exceed 100 characters."));
            }

            if (!string.IsNullOrWhiteSpace(args.NewProduct.Description) && args.NewProduct.Description.Length > 500)
            {
                errors.Add(("description", "Description cannot exceed 500 characters."));
            }

            if (args.NewProduct.Stock < 0)
            {
                errors.Add(("stock", "Stock cannot be negative."));
            }

            if (string.IsNullOrWhiteSpace(args.NewProduct.UnitOfMeasure))
            {
                errors.Add(("unitOfMeasure", "Unit of measure is required."));
            }
            else if (!System.Enum.TryParse<UnitOfMeasure>(args.NewProduct.UnitOfMeasure, ignoreCase: true, out _))
            {
                errors.Add(("unitOfMeasure", "Invalid unit of measure."));
            }

            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }
    }
}

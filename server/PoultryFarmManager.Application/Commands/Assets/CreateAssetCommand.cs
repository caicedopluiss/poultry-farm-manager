using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Application.Commands.Assets;

public sealed class CreateAssetCommand
{
    public record Args(NewAssetDto NewAsset);
    public record Result(AssetDto CreatedAsset);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var asset = args.NewAsset.Map();

            // Create initial state with all quantity as Available
            // EF Core will automatically set the AssetId when the relationship is saved
            asset.States =
            [
                new()
                {
                    Status = AssetStatus.Available,
                    Quantity = args.NewAsset.InitialQuantity
                }
            ];

            var createdAsset = await unitOfWork.Assets.CreateAsync(asset, cancellationToken);

            // Create a transaction for the asset purchase only if vendor and unit price are provided
            if (args.NewAsset.VendorId.HasValue && args.NewAsset.UnitPrice.HasValue)
            {
                var transaction = new Transaction
                {
                    Title = $"Purchase of asset: {createdAsset.Name}",
                    Date = DateTime.UtcNow,
                    Type = TransactionType.Expense,
                    UnitPrice = args.NewAsset.UnitPrice.Value,
                    Quantity = args.NewAsset.InitialQuantity,
                    TransactionAmount = args.NewAsset.UnitPrice.Value * args.NewAsset.InitialQuantity,
                    AssetId = createdAsset.Id,
                    VendorId = args.NewAsset.VendorId.Value,
                    BatchId = null,
                    ProductVariantId = null,
                    CustomerId = null,
                    Notes = "Automatically created during asset registration"
                };

                await unitOfWork.Transactions.CreateAsync(transaction, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            var assetDto = new AssetDto().Map(createdAsset);
            var result = new Result(assetDto);

            return result;
        }

        protected override async Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (string.IsNullOrWhiteSpace(args.NewAsset.Name))
            {
                errors.Add(("name", "Name is required."));
            }
            else if (args.NewAsset.Name.Length > 100)
            {
                errors.Add(("name", "Name cannot exceed 100 characters."));
            }

            if (!string.IsNullOrWhiteSpace(args.NewAsset.Description) && args.NewAsset.Description.Length > 500)
            {
                errors.Add(("description", "Description cannot exceed 500 characters."));
            }

            if (args.NewAsset.InitialQuantity <= 0)
            {
                errors.Add(("initialQuantity", "Initial quantity must be greater than zero."));
            }

            if (!string.IsNullOrWhiteSpace(args.NewAsset.Notes) && args.NewAsset.Notes.Length > 500)
            {
                errors.Add(("notes", "Notes cannot exceed 500 characters."));
            }

            // Vendor is optional, but if provided must exist
            if (args.NewAsset.VendorId.HasValue)
            {
                var vendor = await unitOfWork.Vendors.GetByIdAsync(args.NewAsset.VendorId.Value, cancellationToken: cancellationToken);
                if (vendor == null)
                {
                    errors.Add(("vendorId", "Vendor not found."));
                }
            }

            // Unit price is optional, but if provided must be greater than zero
            if (args.NewAsset.UnitPrice.HasValue && args.NewAsset.UnitPrice.Value <= 0)
            {
                errors.Add(("unitPrice", "Unit price must be greater than zero when provided."));
            }

            // If vendor is provided, unit price should also be provided and vice versa
            if (args.NewAsset.VendorId.HasValue && !args.NewAsset.UnitPrice.HasValue)
            {
                errors.Add(("unitPrice", "Unit price is required when vendor is specified."));
            }

            if (args.NewAsset.UnitPrice.HasValue && !args.NewAsset.VendorId.HasValue)
            {
                errors.Add(("vendorId", "Vendor is required when unit price is specified."));
            }

            return errors;
        }
    }
}

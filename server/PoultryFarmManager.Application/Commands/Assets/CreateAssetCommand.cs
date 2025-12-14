using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;

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
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var assetDto = new AssetDto().Map(createdAsset);
            var result = new Result(assetDto);

            return result;
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
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

            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;
using PoultryFarmManager.Core.Enums;

namespace PoultryFarmManager.Application.Commands.Assets;

public sealed class UpdateAssetCommand
{
    public record Args(Guid AssetId, UpdateAssetDto UpdateData);
    public record Result(AssetDto UpdatedAsset);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var asset = await unitOfWork.Assets.GetByIdAsync(args.AssetId, track: true, cancellationToken: cancellationToken);

            if (asset == null)
            {
                throw new InvalidOperationException($"Asset with ID {args.AssetId} not found.");
            }

            args.UpdateData.ApplyTo(asset);

            await unitOfWork.Assets.UpdateAsync(asset, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var assetDto = new AssetDto().Map(asset);
            var result = new Result(assetDto);

            return result;
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (!string.IsNullOrWhiteSpace(args.UpdateData.Name) && args.UpdateData.Name.Length > 100)
            {
                errors.Add(("name", "Name cannot exceed 100 characters."));
            }

            if (!string.IsNullOrWhiteSpace(args.UpdateData.Description) && args.UpdateData.Description.Length > 500)
            {
                errors.Add(("description", "Description cannot exceed 500 characters."));
            }

            if (!string.IsNullOrWhiteSpace(args.UpdateData.Notes) && args.UpdateData.Notes.Length > 500)
            {
                errors.Add(("notes", "Notes cannot exceed 500 characters."));
            }

            if (args.UpdateData.States != null)
            {
                if (args.UpdateData.States.Count == 0)
                {
                    errors.Add(("states", "At least one asset state is required."));
                }

                // Check for duplicate statuses
                var statusGroups = args.UpdateData.States.GroupBy(s => s.Status?.ToLower()).Where(g => g.Count() > 1);
                foreach (var group in statusGroups)
                {
                    errors.Add(("states", $"Duplicate status '{group.Key}' found. Each status can only appear once."));
                }

                for (int i = 0; i < args.UpdateData.States.Count; i++)
                {
                    var state = args.UpdateData.States[i];

                    if (!Enum.TryParse<AssetStatus>(state.Status, ignoreCase: true, out _))
                    {
                        errors.Add(($"states[{i}].status", "Invalid asset status."));
                    }

                    if (state.Quantity <= 0)
                    {
                        errors.Add(($"states[{i}].quantity", "Quantity must be greater than zero."));
                    }

                    if (!string.IsNullOrWhiteSpace(state.Location) && state.Location.Length > 100)
                    {
                        errors.Add(($"states[{i}].location", "Location cannot exceed 100 characters."));
                    }
                }
            }

            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }
    }
}

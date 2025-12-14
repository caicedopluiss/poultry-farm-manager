using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.Assets;

public sealed class GetAssetByIdQuery
{
    public record Args(Guid AssetId);
    public record Result(AssetDto? Asset);

    public sealed class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var asset = await unitOfWork.Assets.GetByIdAsync(args.AssetId, cancellationToken: cancellationToken);

            if (asset == null) return new Result(null);

            var assetDto = new AssetDto().Map(asset);

            return new Result(assetDto);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<(string field, string error)>());
        }
    }
}

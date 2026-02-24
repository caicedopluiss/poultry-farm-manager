using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application.Queries.Assets;

public static class GetAssetPricingByVendorQuery
{
    public record Args(Guid AssetId);

    public record VendorPricing(
        VendorDto Vendor,
        decimal LastUnitPrice,
        DateTime LastPurchaseDate,
        int TotalPurchases
    );

    public record Result(IEnumerable<VendorPricing> VendorPricings);

    public class Handler(IUnitOfWork unitOfWork) : AppRequestHandler<Args, Result>
    {
        protected override async Task<Result> ExecuteAsync(Args args, CancellationToken cancellationToken = default)
        {
            var transactions = await unitOfWork.Transactions.GetByAssetIdAsync(args.AssetId, cancellationToken);

            var vendorPricings = transactions
                .Where(t => t.Vendor != null)
                .GroupBy(t => t.VendorId)
                .Select(g =>
                {
                    var lastTransaction = g.OrderByDescending(t => t.Date).First();
                    return new VendorPricing(
                        Vendor: new VendorDto().Map(lastTransaction.Vendor!),
                        LastUnitPrice: lastTransaction.UnitPrice,
                        LastPurchaseDate: lastTransaction.Date,
                        TotalPurchases: g.Count()
                    );
                })
                .OrderByDescending(vp => vp.LastPurchaseDate)
                .ToList();

            return new Result(vendorPricings);
        }

        protected override Task<IEnumerable<(string field, string error)>> ValidateAsync(Args args, CancellationToken cancellationToken = default)
        {
            var errors = new List<(string field, string error)>();

            if (args.AssetId == Guid.Empty)
            {
                errors.Add(("assetId", "Asset ID is required."));
            }

            return Task.FromResult<IEnumerable<(string field, string error)>>(errors);
        }
    }
}

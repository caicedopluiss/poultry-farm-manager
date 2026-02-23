using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.Queries.Assets;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.Assets;

public sealed class GetAssetPricingByVendorEndpoint : IEndpoint
{
    public record GetAssetPricingByVendorResponseBody(IEnumerable<GetAssetPricingByVendorQuery.VendorPricing> VendorPricings);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "assets", "{id:guid}", "pricing-by-vendor");
        app.MapGet(route, HandleAsync)
            .WithName(nameof(GetAssetPricingByVendorEndpoint))
            .WithTags(nameof(Assets))
            .Produces<GetAssetPricingByVendorResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestHandler<GetAssetPricingByVendorQuery.Args, GetAssetPricingByVendorQuery.Result> handler,
        CancellationToken cancellationToken)
    {
        var request = new AppRequest<GetAssetPricingByVendorQuery.Args>(new(id));
        var response = await handler.HandleAsync(request, cancellationToken);

        if (!response.IsSuccess)
        {
            return Results.BadRequest(new ErrorResponse(response.ValidationErrors));
        }

        return Results.Ok(new GetAssetPricingByVendorResponseBody(response.Value!.VendorPricings));
    }
}

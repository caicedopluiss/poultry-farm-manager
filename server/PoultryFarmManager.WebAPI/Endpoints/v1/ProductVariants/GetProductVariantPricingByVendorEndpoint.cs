using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.Queries.ProductVariants;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.ProductVariants;

public sealed class GetProductVariantPricingByVendorEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "product-variants", "{id:guid}", "pricing-by-vendor");
        app.MapGet(route, HandleAsync)
            .WithName(nameof(GetProductVariantPricingByVendorEndpoint))
            .WithTags(nameof(ProductVariants))
            .Produces<GetProductVariantPricingByVendorQuery.Result>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestHandler<GetProductVariantPricingByVendorQuery.Args, GetProductVariantPricingByVendorQuery.Result> handler,
        CancellationToken cancellationToken)
    {
        var request = new AppRequest<GetProductVariantPricingByVendorQuery.Args>(new(id));
        var response = await handler.HandleAsync(request, cancellationToken);

        if (!response.IsSuccess)
        {
            return Results.BadRequest(new ErrorResponse(response.ValidationErrors));
        }

        return Results.Ok(response.Value);
    }
}

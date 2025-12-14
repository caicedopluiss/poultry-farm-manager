using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Queries.ProductVariants;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.ProductVariants;

public class GetProductVariantsByProductIdEndpoint : IEndpoint
{
    public record GetProductVariantsByProductIdResponseBody(IEnumerable<ProductVariantDto> ProductVariants);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "products", "{productId:guid}", "variants");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetProductVariantsByProductIdEndpoint))
            .WithTags(nameof(ProductVariants))
            .Produces<GetProductVariantsByProductIdResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid productId,
        [FromServices] IAppRequestsMediator mediator
    )
    {
        var request = new AppRequest<GetProductVariantsByProductIdQuery.Args>(new(productId));
        var result = await mediator.SendAsync<GetProductVariantsByProductIdQuery.Args, GetProductVariantsByProductIdQuery.Result>(request);

        return !result.IsSuccess
            ? Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors))
            : Results.Ok(new GetProductVariantsByProductIdResponseBody(result.Value!.ProductVariants));
    }
}

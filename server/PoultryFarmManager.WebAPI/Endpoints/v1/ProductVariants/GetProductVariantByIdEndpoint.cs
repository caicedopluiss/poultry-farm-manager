using System;
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

public class GetProductVariantByIdEndpoint : IEndpoint
{
    public record GetProductVariantByIdResponseBody(ProductVariantDto ProductVariant);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "product-variants", "{id:guid}");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetProductVariantByIdEndpoint))
            .WithTags(nameof(ProductVariants))
            .Produces<GetProductVariantByIdResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator
    )
    {
        var request = new AppRequest<GetProductVariantByIdQuery.Args>(new(id));
        var result = await mediator.SendAsync<GetProductVariantByIdQuery.Args, GetProductVariantByIdQuery.Result>(request);

        if (!result.IsSuccess) return Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
        if (result.Value?.ProductVariant is null) return Results.NotFound();

        return Results.Ok(new GetProductVariantByIdResponseBody(result.Value.ProductVariant));
    }
}

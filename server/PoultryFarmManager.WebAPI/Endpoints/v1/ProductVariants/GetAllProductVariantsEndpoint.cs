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

public class GetAllProductVariantsEndpoint : IEndpoint
{
    public record GetAllProductVariantsResponseBody(IEnumerable<ProductVariantDto> ProductVariants);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "product-variants");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetAllProductVariantsEndpoint))
            .WithTags(nameof(ProductVariants))
            .Produces<GetAllProductVariantsResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> GetAsync(
        [FromServices] IAppRequestsMediator mediator
    )
    {
        var request = new AppRequest<GetAllProductVariantsQuery.Args>(new());
        var result = await mediator.SendAsync<GetAllProductVariantsQuery.Args, GetAllProductVariantsQuery.Result>(request);

        return !result.IsSuccess
            ? Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors))
            : Results.Ok(new GetAllProductVariantsResponseBody(result.Value!.ProductVariants));
    }
}

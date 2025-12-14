using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Queries.Products;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.Products;

public class GetAllProductsEndpoint : IEndpoint
{
    public record GetAllProductsResponseBody(IEnumerable<ProductDto> Products);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "products");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetAllProductsEndpoint))
            .WithTags(nameof(Products))
            .Produces<GetAllProductsResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> GetAsync(
        [FromServices] IAppRequestsMediator mediator
    )
    {
        var request = new AppRequest<GetAllProductsQuery.Args>(new());
        var result = await mediator.SendAsync<GetAllProductsQuery.Args, GetAllProductsQuery.Result>(request);

        return !result.IsSuccess
            ? Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors))
            : Results.Ok(new GetAllProductsResponseBody(result.Value!.Products));
    }
}

using System;
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

public class GetProductByIdEndpoint : IEndpoint
{
    public record GetProductByIdResponseBody(ProductDto Product);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "products", "{id:guid}");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetProductByIdEndpoint))
            .WithTags(nameof(Products))
            .Produces<GetProductByIdResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator
    )
    {
        var request = new AppRequest<GetProductByIdQuery.Args>(new(id));
        var result = await mediator.SendAsync<GetProductByIdQuery.Args, GetProductByIdQuery.Result>(request);

        if (!result.IsSuccess) return Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
        if (result.Value?.Product is null) return Results.NotFound();

        return Results.Ok(new GetProductByIdResponseBody(result.Value.Product));
    }
}

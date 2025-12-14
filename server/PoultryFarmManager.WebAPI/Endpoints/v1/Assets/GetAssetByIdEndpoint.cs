using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Queries.Assets;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.Assets;

public class GetAssetByIdEndpoint : IEndpoint
{
    public record GetAssetByIdResponseBody(AssetDto Asset);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "assets", "{id:guid}");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetAssetByIdEndpoint))
            .WithTags(nameof(Assets))
            .Produces<GetAssetByIdResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator
    )
    {
        var request = new AppRequest<GetAssetByIdQuery.Args>(new(id));
        var result = await mediator.SendAsync<GetAssetByIdQuery.Args, GetAssetByIdQuery.Result>(request);

        if (!result.IsSuccess) return Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
        if (result.Value?.Asset is null) return Results.NotFound();

        return Results.Ok(new GetAssetByIdResponseBody(result.Value.Asset));
    }
}

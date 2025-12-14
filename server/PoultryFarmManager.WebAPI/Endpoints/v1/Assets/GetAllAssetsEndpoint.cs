using System.Collections.Generic;
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

public class GetAllAssetsEndpoint : IEndpoint
{
    public record GetAllAssetsResponseBody(IEnumerable<AssetDto> Assets);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "assets");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetAllAssetsEndpoint))
            .WithTags(nameof(Assets))
            .Produces<GetAllAssetsResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> GetAsync(
        [FromServices] IAppRequestsMediator mediator
    )
    {
        var request = new AppRequest<GetAllAssetsQuery.Args>(new());
        var result = await mediator.SendAsync<GetAllAssetsQuery.Args, GetAllAssetsQuery.Result>(request);

        return !result.IsSuccess
            ? Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors))
            : Results.Ok(new GetAllAssetsResponseBody(result.Value!.Assets));
    }
}

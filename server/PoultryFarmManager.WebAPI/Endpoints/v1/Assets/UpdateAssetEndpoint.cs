using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.Commands.Assets;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.Assets;

public class UpdateAssetEndpoint : IEndpoint
{
    public record UpdateAssetRequestBody(UpdateAssetDto UpdateAsset);
    public record UpdateAssetResponseBody(AssetDto Asset);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "assets", "{id:guid}");
        app.MapPut(route, UpdateAsync)
            .WithName(nameof(UpdateAssetEndpoint))
            .WithTags(nameof(Assets))
            .Accepts<UpdateAssetRequestBody>(MediaTypeNames.Application.Json)
            .Produces<UpdateAssetResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] UpdateAssetRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<UpdateAssetCommand.Args>(new(id, body.UpdateAsset));
        try
        {
            var result = await mediator.SendAsync<UpdateAssetCommand.Args, UpdateAssetCommand.Result>(request, cancellationToken);

            return !result.IsSuccess ?
                Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors)) :
                Results.Ok(new UpdateAssetResponseBody(result.Value!.UpdatedAsset));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound();
        }
    }
}

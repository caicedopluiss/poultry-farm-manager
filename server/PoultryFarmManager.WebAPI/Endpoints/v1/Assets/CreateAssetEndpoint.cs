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

public class CreateAssetEndpoint : IEndpoint
{
    public record CreateAssetRequestBody(NewAssetDto NewAsset);
    public record CreateAssetResponseBody(AssetDto Asset);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "assets");
        app.MapPost(route, CreateAsync)
            .WithName(nameof(CreateAssetEndpoint))
            .WithTags(nameof(Assets))
            .Accepts<CreateAssetRequestBody>(MediaTypeNames.Application.Json)
            .Produces<CreateAssetResponseBody>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> CreateAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] CreateAssetRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<CreateAssetCommand.Args>(new(body.NewAsset));
        var result = await mediator.SendAsync<CreateAssetCommand.Args, CreateAssetCommand.Result>(request, cancellationToken);

        return !result.IsSuccess ?
            Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors)) :
            Results.Created($"/api/v1/assets/{result.Value!.CreatedAsset.Id}", new CreateAssetResponseBody(result.Value!.CreatedAsset));
    }
}

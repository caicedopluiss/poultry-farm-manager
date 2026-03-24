using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.Commands.FeedingTables;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.FeedingTables;

public class UpdateFeedingTableEndpoint : IEndpoint
{
    public record UpdateFeedingTableRequestBody(UpdateFeedingTableDto Updates);
    public record UpdateFeedingTableResponseBody(FeedingTableDto FeedingTable);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "feeding-tables", "{id:guid}");
        app.MapPatch(route, UpdateAsync)
            .WithName(nameof(UpdateFeedingTableEndpoint))
            .WithTags(nameof(FeedingTables))
            .Accepts<UpdateFeedingTableRequestBody>(MediaTypeNames.Application.Json)
            .Produces<UpdateFeedingTableResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> UpdateAsync(
        [FromServices] IAppRequestsMediator mediator,
        Guid id,
        [FromBody] UpdateFeedingTableRequestBody body,
        CancellationToken ct)
    {
        var request = new AppRequest<UpdateFeedingTableCommand.Args>(new(id, body.Updates));
        var result = await mediator.SendAsync<UpdateFeedingTableCommand.Args, UpdateFeedingTableCommand.Result>(request, ct);

        if (!result.IsSuccess)
        {
            var isNotFound = result.ValidationErrors.Any(e => e.error.Contains("not found", StringComparison.OrdinalIgnoreCase));
            return isNotFound
                ? Results.NotFound(new ErrorResponse(result.Message, result.ValidationErrors))
                : Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
        }

        return Results.Ok(new UpdateFeedingTableResponseBody(result.Value!.UpdatedFeedingTable));
    }
}

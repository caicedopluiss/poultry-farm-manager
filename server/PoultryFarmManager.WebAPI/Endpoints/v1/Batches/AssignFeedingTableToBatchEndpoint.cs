using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.Commands.Batches;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.Batches;

public class AssignFeedingTableToBatchEndpoint : IEndpoint
{
    public record AssignFeedingTableToBatchRequestBody(Guid? FeedingTableId);
    public record AssignFeedingTableToBatchResponseBody(BatchDto Batch);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "batches", "{id:guid}", "feeding-table");
        app.MapPatch(route, AssignAsync)
            .WithName(nameof(AssignFeedingTableToBatchEndpoint))
            .WithTags(nameof(Batches))
            .Accepts<AssignFeedingTableToBatchRequestBody>(MediaTypeNames.Application.Json)
            .Produces<AssignFeedingTableToBatchResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> AssignAsync(
        [FromServices] IAppRequestsMediator mediator,
        Guid id,
        [FromBody] AssignFeedingTableToBatchRequestBody body,
        CancellationToken ct)
    {
        var request = new AppRequest<AssignFeedingTableToBatchCommand.Args>(new(id, body.FeedingTableId));
        var result = await mediator.SendAsync<AssignFeedingTableToBatchCommand.Args, AssignFeedingTableToBatchCommand.Result>(request, ct);

        if (!result.IsSuccess)
        {
            var isNotFound = result.ValidationErrors.Any(e => e.error.Contains("not found", StringComparison.OrdinalIgnoreCase));
            return isNotFound
                ? Results.NotFound(new ErrorResponse(result.Message, result.ValidationErrors))
                : Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
        }

        return Results.Ok(new AssignFeedingTableToBatchResponseBody(result.Value!.UpdatedBatch));
    }
}

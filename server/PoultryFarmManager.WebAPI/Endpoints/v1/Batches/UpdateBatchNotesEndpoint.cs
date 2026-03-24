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
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.Batches;

public class UpdateBatchNotesEndpoint : IEndpoint
{
    public record UpdateBatchNotesRequestBody(string? Notes);
    public record UpdateBatchNotesResponseBody(bool Success);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "batches", "{id:guid}", "notes");
        app.MapPut(route, UpdateAsync)
            .WithName(nameof(UpdateBatchNotesEndpoint))
            .WithTags(nameof(Batches))
            .Accepts<UpdateBatchNotesRequestBody>(MediaTypeNames.Application.Json)
            .Produces<UpdateBatchNotesResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> UpdateAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromRoute] Guid id,
        [FromBody] UpdateBatchNotesRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<UpdateBatchNotesCommand.Args>(new(id, body.Notes));
        var result = await mediator.SendAsync<UpdateBatchNotesCommand.Args, UpdateBatchNotesCommand.Result>(request, cancellationToken);

        if (!result.IsSuccess)
        {
            var isNotFound = result.ValidationErrors.Any(e => e.error.Contains("not found", StringComparison.OrdinalIgnoreCase));
            return isNotFound
                ? Results.NotFound(new ErrorResponse(result.Message, result.ValidationErrors))
                : Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
        }

        return Results.Ok(new UpdateBatchNotesResponseBody(true));
    }
}

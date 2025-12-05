using System;
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

public class SwitchBatchStatusEndpoint : IEndpoint
{
    public record SwitchStatusRequestBody(NewStatusSwitchDto StatusSwitch);
    public record SwitchStatusResponseBody(StatusSwitchActivityDto StatusSwitch);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "batches", "{id:guid}", "status");
        app.MapPost(route, SwitchStatusAsync)
            .WithName(nameof(SwitchBatchStatusEndpoint))
            .WithTags(nameof(Batches))
            .Accepts<SwitchStatusRequestBody>(MediaTypeNames.Application.Json)
            .Produces<SwitchStatusResponseBody>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> SwitchStatusAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromRoute] Guid id,
        [FromBody] SwitchStatusRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<SwitchBatchStatusCommand.Args>(new(id, body.StatusSwitch));
        try
        {
            var result = await mediator.SendAsync<SwitchBatchStatusCommand.Args, SwitchBatchStatusCommand.Result>(request, cancellationToken);

            return !result.IsSuccess ?
                Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors)) :
                Results.Created(
                    $"/api/v1/batches/{result.Value!.StatusSwitch.BatchId}/status/{result.Value!.StatusSwitch.Id}",
                    new SwitchStatusResponseBody(result.Value!.StatusSwitch));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new ErrorResponse(StatusCodes.Status404NotFound, ex.Message));
        }
    }
}

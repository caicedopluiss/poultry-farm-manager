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

public class RegisterWeightMeasurementEndpoint : IEndpoint
{
    public record RegisterWeightMeasurementRequestBody(NewWeightMeasurementDto WeightMeasurement);
    public record RegisterWeightMeasurementResponseBody(WeightMeasurementActivityDto WeightMeasurement);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "batches", "{id:guid}", "weight-measurements");
        app.MapPost(route, RegisterAsync)
            .WithName(nameof(RegisterWeightMeasurementEndpoint))
            .WithTags(nameof(Batches))
            .Accepts<RegisterWeightMeasurementRequestBody>(MediaTypeNames.Application.Json)
            .Produces<RegisterWeightMeasurementResponseBody>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> RegisterAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromRoute] Guid id,
        [FromBody] RegisterWeightMeasurementRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<RegisterWeightMeasurementCommand.Args>(new(id, body.WeightMeasurement));
        try
        {
            var result = await mediator.SendAsync<RegisterWeightMeasurementCommand.Args, RegisterWeightMeasurementCommand.Result>(request, cancellationToken);

            return !result.IsSuccess ?
                Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors)) :
                Results.Created(
                    $"/api/v1/batches/{result.Value!.WeightMeasurement.BatchId}/weight-measurements/{result.Value!.WeightMeasurement.Id}",
                    new RegisterWeightMeasurementResponseBody(result.Value!.WeightMeasurement));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new ErrorResponse(StatusCodes.Status404NotFound, ex.Message));
        }
    }
}

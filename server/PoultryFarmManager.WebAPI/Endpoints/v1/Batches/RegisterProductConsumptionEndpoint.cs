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

public class RegisterProductConsumptionEndpoint : IEndpoint
{
    public record RegisterProductConsumptionRequestBody(NewProductConsumptionDto ProductConsumption);
    public record RegisterProductConsumptionResponseBody(ProductConsumptionActivityDto ProductConsumption);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "batches", "{id:guid}", "product-consumption");
        app.MapPost(route, RegisterAsync)
            .WithName(nameof(RegisterProductConsumptionEndpoint))
            .WithTags(nameof(Batches))
            .Accepts<RegisterProductConsumptionRequestBody>(MediaTypeNames.Application.Json)
            .Produces<RegisterProductConsumptionResponseBody>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> RegisterAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromRoute] Guid id,
        [FromBody] RegisterProductConsumptionRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<RegisterProductConsumptionCommand.Args>(new(id, body.ProductConsumption));
        try
        {
            var result = await mediator.SendAsync<RegisterProductConsumptionCommand.Args, RegisterProductConsumptionCommand.Result>(request, cancellationToken);

            return !result.IsSuccess ?
                Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors)) :
                Results.Created(
                    $"/api/v1/batches/{result.Value!.ProductConsumption.BatchId}/product-consumption/{result.Value!.ProductConsumption.Id}",
                    new RegisterProductConsumptionResponseBody(result.Value!.ProductConsumption));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new ErrorResponse(StatusCodes.Status404NotFound, ex.Message));
        }
    }
}

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

public class RegisterMortalityEndpoint : IEndpoint
{
    public record RegisterMortalityRequestBody(NewMortalityRegistrationDto MortalityRegistration);
    public record RegisterMortalityResponseBody(MortalityRegistrationActivityDto MortalityRegistration);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "batches", "{id:guid}", "mortality");
        app.MapPost(route, RegisterAsync)
            .WithName(nameof(RegisterMortalityEndpoint))
            .WithTags(nameof(Batches))
            .Accepts<RegisterMortalityRequestBody>(MediaTypeNames.Application.Json)
            .Produces<RegisterMortalityResponseBody>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> RegisterAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromRoute] Guid id,
        [FromBody] RegisterMortalityRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<RegisterMortalityCommand.Args>(new(id, body.MortalityRegistration));
        var result = await mediator.SendAsync<RegisterMortalityCommand.Args, RegisterMortalityCommand.Result>(request, cancellationToken);

        if (!result.IsSuccess)
        {
            var isNotFound = result.ValidationErrors.Any(e => e.error.Contains("not found", StringComparison.OrdinalIgnoreCase));
            return isNotFound
                ? Results.NotFound(new ErrorResponse(result.Message, result.ValidationErrors))
                : Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
        }

        return Results.Created(
            $"/api/v1/batches/{result.Value!.MortalityRegistration.BatchId}/mortality/{result.Value!.MortalityRegistration.Id}",
            new RegisterMortalityResponseBody(result.Value!.MortalityRegistration));
    }
}

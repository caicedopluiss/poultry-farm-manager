using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.Commands.Persons;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.Persons;

/// <summary>
/// Update an existing person
/// </summary>
public class UpdatePersonEndpoint : IEndpoint
{
    public record UpdatePersonRequestBody(UpdatePersonDto Person);
    public record UpdatePersonResponseBody(PersonDto Person);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "persons", "{id:guid}");
        app.MapPut(route, UpdateAsync)
            .WithName(nameof(UpdatePersonEndpoint))
            .WithTags(nameof(Persons))
            .Accepts<UpdatePersonRequestBody>(MediaTypeNames.Application.Json)
            .Produces<UpdatePersonResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] UpdatePersonRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<UpdatePersonCommand.Args>(new(id, body.Person));

        try
        {
            var result = await mediator.SendAsync<UpdatePersonCommand.Args, UpdatePersonCommand.Result>(request, cancellationToken);

            if (!result.IsSuccess)
            {
                return Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
            }

            return Results.Ok(new UpdatePersonResponseBody(result.Value!.UpdatedPerson));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }
}

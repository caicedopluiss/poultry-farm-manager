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

public class CreatePersonEndpoint : IEndpoint
{
    public record CreatePersonRequestBody(NewPersonDto Person);
    public record CreatePersonResponseBody(PersonDto Person);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "persons");
        app.MapPost(route, CreateAsync)
            .WithName(nameof(CreatePersonEndpoint))
            .WithTags(nameof(Persons))
            .Accepts<CreatePersonRequestBody>(MediaTypeNames.Application.Json)
            .Produces<CreatePersonResponseBody>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> CreateAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] CreatePersonRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<CreatePersonCommand.Args>(new(body.Person));

        var result = await mediator.SendAsync<CreatePersonCommand.Args, CreatePersonCommand.Result>(request, cancellationToken);

        return !result.IsSuccess ?
            Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors)) :
            Results.Created(
                $"/api/v1/persons/{result.Value!.CreatedPerson.Id}",
                new CreatePersonResponseBody(result.Value!.CreatedPerson));
    }
}

using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Queries.Persons;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.Persons;

public class GetPersonByIdEndpoint : IEndpoint
{
    public record GetPersonByIdResponseBody(PersonDto Person);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "persons", "{id:guid}");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetPersonByIdEndpoint))
            .WithTags(nameof(Persons))
            .Produces<GetPersonByIdResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator
    )
    {
        var request = new AppRequest<GetPersonByIdQuery.Args>(new(id));
        var result = await mediator.SendAsync<GetPersonByIdQuery.Args, GetPersonByIdQuery.Result>(request);

        if (!result.IsSuccess) return Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
        if (result.Value?.Person is null) return Results.NotFound();

        return Results.Ok(new GetPersonByIdResponseBody(result.Value.Person));
    }
}

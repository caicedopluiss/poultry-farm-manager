using System.Collections.Generic;
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

public class GetAllPersonsEndpoint : IEndpoint
{
    public record GetAllPersonsResponseBody(IEnumerable<PersonDto> Persons);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "persons");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetAllPersonsEndpoint))
            .WithTags(nameof(Persons))
            .Produces<GetAllPersonsResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> GetAsync(
        [FromServices] IAppRequestsMediator mediator
    )
    {
        var request = new AppRequest<GetAllPersonsQuery.Args>(new());
        var result = await mediator.SendAsync<GetAllPersonsQuery.Args, GetAllPersonsQuery.Result>(request);

        return !result.IsSuccess
            ? Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors))
            : Results.Ok(new GetAllPersonsResponseBody(result.Value!.Persons));
    }
}

using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Queries.FeedingTables;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.FeedingTables;

public class GetFeedingTableByIdEndpoint : IEndpoint
{
    public record GetFeedingTableByIdResponseBody(FeedingTableDto FeedingTable);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "feeding-tables", "{id:guid}");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetFeedingTableByIdEndpoint))
            .WithTags(nameof(FeedingTables))
            .Produces<GetFeedingTableByIdResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAsync(
        [FromServices] IAppRequestsMediator mediator,
        Guid id,
        CancellationToken ct)
    {
        var request = new AppRequest<GetFeedingTableByIdQuery.Args>(new(id));
        var result = await mediator.SendAsync<GetFeedingTableByIdQuery.Args, GetFeedingTableByIdQuery.Result>(request, ct);
        if (!result.IsSuccess)
        {
            return Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
        }
        return result.Value!.FeedingTable is null
            ? Results.NotFound()
            : Results.Ok(new GetFeedingTableByIdResponseBody(result.Value.FeedingTable));
    }
}

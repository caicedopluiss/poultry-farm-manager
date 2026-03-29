using System.Collections.Generic;
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

public class GetFeedingTablesListEndpoint : IEndpoint
{
    public record GetFeedingTablesListResponseBody(IEnumerable<FeedingTableDto> FeedingTables);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        app.MapGet(Utils.BuildEndpointRoute(prefix, "v1", "feeding-tables"), GetAsync)
            .WithName(nameof(GetFeedingTablesListEndpoint))
            .WithTags(nameof(FeedingTables))
            .Produces<GetFeedingTablesListResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> GetAsync(
        [FromServices] IAppRequestsMediator mediator,
        CancellationToken ct)
    {
        var request = new AppRequest<GetFeedingTablesListQuery.Args>(new());
        var result = await mediator.SendAsync<GetFeedingTablesListQuery.Args, GetFeedingTablesListQuery.Result>(request, ct);
        return Results.Ok(new GetFeedingTablesListResponseBody(result.Value!.FeedingTables));
    }
}

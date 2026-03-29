using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.Commands.FeedingTables;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.FeedingTables;

public class CreateFeedingTableEndpoint : IEndpoint
{
    public record CreateFeedingTableRequestBody(NewFeedingTableDto NewFeedingTable);
    public record CreateFeedingTableResponseBody(FeedingTableDto FeedingTable);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        app.MapPost(Utils.BuildEndpointRoute(prefix, "v1", "feeding-tables"), CreateAsync)
            .WithName(nameof(CreateFeedingTableEndpoint))
            .WithTags(nameof(FeedingTables))
            .Accepts<CreateFeedingTableRequestBody>(MediaTypeNames.Application.Json)
            .Produces<CreateFeedingTableResponseBody>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> CreateAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] CreateFeedingTableRequestBody body,
        CancellationToken ct)
    {
        var request = new AppRequest<CreateFeedingTableCommand.Args>(new(body.NewFeedingTable));
        var result = await mediator.SendAsync<CreateFeedingTableCommand.Args, CreateFeedingTableCommand.Result>(request, ct);
        return !result.IsSuccess
            ? Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors))
            : Results.Created($"/api/v1/feeding-tables/{result.Value!.CreatedFeedingTable.Id}", new CreateFeedingTableResponseBody(result.Value!.CreatedFeedingTable));
    }
}

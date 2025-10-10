using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Queries.Batches;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.Batches;

public class GetBatchByIdEndpoint : IEndpoint
{
    public record GetBatchByIdResponseBody(BatchDto? Batch);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "batches", "{id:guid}");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetBatchByIdEndpoint))
            .WithTags(nameof(Batches))
            .Produces<GetBatchByIdResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator
    )
    {
        var request = new AppRequest<GetBatchByIdQuery.Args>(new(id));
        var result = await mediator.SendAsync<GetBatchByIdQuery.Args, GetBatchByIdQuery.Result>(request);

        if (!result.IsSuccess) return Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
        if (result.Value?.Batch == null) return Results.NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Batch not found"));

        return Results.Ok(new GetBatchByIdResponseBody(result.Value.Batch));
    }
}

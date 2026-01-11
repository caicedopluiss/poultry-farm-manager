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

public class UpdateBatchNameEndpoint : IEndpoint
{
    public record UpdateBatchNameRequestBody(string Name);
    public record UpdateBatchNameResponseBody(BatchDto Batch);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "batches", "{id:guid}", "name");
        app.MapPut(route, UpdateAsync)
            .WithName(nameof(UpdateBatchNameEndpoint))
            .WithTags(nameof(Batches))
            .Accepts<UpdateBatchNameRequestBody>(MediaTypeNames.Application.Json)
            .Produces<UpdateBatchNameResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> UpdateAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromRoute] Guid id,
        [FromBody] UpdateBatchNameRequestBody body,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AppRequest<UpdateBatchNameCommand.Args>(new(id, body.Name));
            var result = await mediator.SendAsync<UpdateBatchNameCommand.Args, UpdateBatchNameCommand.Result>(request, cancellationToken);

            if (!result.IsSuccess)
            {
                return Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
            }

            return Results.Ok(new UpdateBatchNameResponseBody(result.Value!.UpdatedBatch));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new ErrorResponse(404, "Batch not found."));
        }
    }
}

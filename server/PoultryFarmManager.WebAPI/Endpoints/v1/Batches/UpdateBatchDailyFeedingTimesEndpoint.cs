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

public class UpdateBatchDailyFeedingTimesEndpoint : IEndpoint
{
    public record UpdateBatchDailyFeedingTimesRequestBody(int? DailyFeedingTimes);
    public record UpdateBatchDailyFeedingTimesResponseBody(BatchDto Batch);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "batches", "{id:guid}", "daily-feeding-times");
        app.MapPatch(route, UpdateAsync)
            .WithName(nameof(UpdateBatchDailyFeedingTimesEndpoint))
            .WithTags(nameof(Batches))
            .Accepts<UpdateBatchDailyFeedingTimesRequestBody>(MediaTypeNames.Application.Json)
            .Produces<UpdateBatchDailyFeedingTimesResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> UpdateAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromRoute] Guid id,
        [FromBody] UpdateBatchDailyFeedingTimesRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<UpdateBatchDailyFeedingTimesCommand.Args>(new(id, body.DailyFeedingTimes));
        var result = await mediator.SendAsync<UpdateBatchDailyFeedingTimesCommand.Args, UpdateBatchDailyFeedingTimesCommand.Result>(request, cancellationToken);

        if (!result.IsSuccess)
        {
            var isNotFound = result.ValidationErrors.Any(e => e.error.Contains("not found", StringComparison.OrdinalIgnoreCase));
            return isNotFound
                ? Results.NotFound(new ErrorResponse(result.Message, result.ValidationErrors))
                : Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
        }

        return Results.Ok(new UpdateBatchDailyFeedingTimesResponseBody(result.Value!.UpdatedBatch));
    }
}

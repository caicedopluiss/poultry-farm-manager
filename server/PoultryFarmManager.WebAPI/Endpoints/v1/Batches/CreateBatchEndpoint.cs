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

public class CreateBatchEndpoint : IEndpoint
{
    public record RequestBody(NewBatchDto NewBatch);
    public record ResponseBody(BatchDto Batch);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "api", "v1", "batches");
        app.MapPost(route, CreateAsync)
            .WithName(nameof(CreateBatchEndpoint))
            .WithTags(nameof(Batches))
            .Accepts<RequestBody>(MediaTypeNames.Application.Json)
            .Produces<ResponseBody>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> CreateAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] RequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<CreateBatchCommand.Args>(new(body.NewBatch));
        var result = await mediator.SendAsync<CreateBatchCommand.Args, CreateBatchCommand.Result>(request, cancellationToken);

        return !result.IsSuccess ?
            Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors)) :
            Results.Created($"/api/v1/batches/{result.Value?.CreatedBatch.Id}", new ResponseBody(result.Value?.CreatedBatch!));
    }
}

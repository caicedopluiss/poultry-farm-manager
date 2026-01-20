using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.Commands.Transactions;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.Transactions;

public class CreateTransactionEndpoint : IEndpoint
{
    public record CreateTransactionRequestBody(NewTransactionDto Transaction);
    public record CreateTransactionResponseBody(TransactionDto Transaction);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "transactions");
        app.MapPost(route, CreateAsync)
            .WithName(nameof(CreateTransactionEndpoint))
            .WithTags(nameof(Transactions))
            .Accepts<CreateTransactionRequestBody>(MediaTypeNames.Application.Json)
            .Produces<CreateTransactionResponseBody>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> CreateAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] CreateTransactionRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<CreateTransactionCommand.Args>(new(body.Transaction));

        var result = await mediator.SendAsync<CreateTransactionCommand.Args, CreateTransactionCommand.Result>(request, cancellationToken);

        return !result.IsSuccess ?
            Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors)) :
            Results.Created(
                $"/api/v1/transactions/{result.Value!.CreatedTransaction.Id}",
                new CreateTransactionResponseBody(result.Value!.CreatedTransaction));
    }
}

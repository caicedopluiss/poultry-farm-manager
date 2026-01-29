using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Queries.Transactions;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.Transactions;

public class GetBatchTransactionsEndpoint : IEndpoint
{
    public record GetBatchTransactionsResponseBody(TransactionDto[] Transactions);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "batches", "{id:guid}", "transactions");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetBatchTransactionsEndpoint))
            .WithTags("Transactions")
            .WithDescription("Get all transactions for a specific batch")
            .Produces<GetBatchTransactionsResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator)
    {
        var request = new AppRequest<GetBatchTransactionsQuery.Args>(new(id));
        var result = await mediator.SendAsync<GetBatchTransactionsQuery.Args, GetBatchTransactionsQuery.Result>(request);

        if (!result.IsSuccess)
            return Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));

        return Results.Ok(new GetBatchTransactionsResponseBody(result.Value!.Transactions.ToArray()));
    }
}

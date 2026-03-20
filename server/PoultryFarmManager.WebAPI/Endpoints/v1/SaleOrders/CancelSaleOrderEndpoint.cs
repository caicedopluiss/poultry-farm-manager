using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.Commands.SaleOrders;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.SaleOrders;

public class CancelSaleOrderEndpoint : IEndpoint
{
    public record CancelSaleOrderResponseBody(SaleOrderDto SaleOrder);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "sale-orders", "{id:guid}", "cancel");
        app.MapPost(route, CancelAsync)
            .WithName(nameof(CancelSaleOrderEndpoint))
            .WithTags("SaleOrders")
            .Produces<CancelSaleOrderResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> CancelAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<CancelSaleOrderCommand.Args>(new(id));
        var result = await mediator.SendAsync<CancelSaleOrderCommand.Args, CancelSaleOrderCommand.Result>(request, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ValidationErrors.Any(e => e.field == "saleOrderId" && e.error.Contains("not found")))
                return Results.NotFound(new ErrorResponse(StatusCodes.Status404NotFound, "Sale order not found."));

            return Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
        }

        return Results.Ok(new CancelSaleOrderResponseBody(result.Value!.CancelledSaleOrder));
    }
}

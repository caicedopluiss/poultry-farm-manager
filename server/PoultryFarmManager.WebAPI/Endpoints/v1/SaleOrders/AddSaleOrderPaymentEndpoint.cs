using System;
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

public class AddSaleOrderPaymentEndpoint : IEndpoint
{
    public record AddPaymentRequestBody(AddSaleOrderPaymentDto Payment);
    public record AddPaymentResponseBody(SaleOrderDto SaleOrder);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "sale-orders", "{id:guid}", "payments");
        app.MapPost(route, AddAsync)
            .WithName(nameof(AddSaleOrderPaymentEndpoint))
            .WithTags("SaleOrders")
            .Accepts<AddPaymentRequestBody>(MediaTypeNames.Application.Json)
            .Produces<AddPaymentResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> AddAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] AddPaymentRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<AddSaleOrderPaymentCommand.Args>(new(id, body.Payment));
        var result = await mediator.SendAsync<AddSaleOrderPaymentCommand.Args, AddSaleOrderPaymentCommand.Result>(request, cancellationToken);

        return !result.IsSuccess
            ? Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors))
            : Results.Ok(new AddPaymentResponseBody(result.Value!.UpdatedSaleOrder));
    }
}

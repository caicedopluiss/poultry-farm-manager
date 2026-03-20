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

public class CreateSaleOrderEndpoint : IEndpoint
{
    public record CreateSaleOrderRequestBody(NewSaleOrderDto NewSaleOrder);
    public record CreateSaleOrderResponseBody(SaleOrderDto SaleOrder);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "sale-orders");
        app.MapPost(route, CreateAsync)
            .WithName(nameof(CreateSaleOrderEndpoint))
            .WithTags("SaleOrders")
            .Accepts<CreateSaleOrderRequestBody>(MediaTypeNames.Application.Json)
            .Produces<CreateSaleOrderResponseBody>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> CreateAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] CreateSaleOrderRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<CreateSaleOrderCommand.Args>(new(body.NewSaleOrder));
        var result = await mediator.SendAsync<CreateSaleOrderCommand.Args, CreateSaleOrderCommand.Result>(request, cancellationToken);

        return !result.IsSuccess
            ? Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors))
            : Results.Created($"/api/v1/sale-orders/{result.Value!.CreatedSaleOrder.Id}", new CreateSaleOrderResponseBody(result.Value!.CreatedSaleOrder));
    }
}

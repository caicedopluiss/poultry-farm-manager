using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Queries.SaleOrders;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.SaleOrders;

public class GetSaleOrderByIdEndpoint : IEndpoint
{
    public record GetSaleOrderByIdResponseBody(SaleOrderDto SaleOrder);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "sale-orders", "{id:guid}");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetSaleOrderByIdEndpoint))
            .WithTags("SaleOrders")
            .Produces<GetSaleOrderByIdResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator)
    {
        var request = new AppRequest<GetSaleOrderByIdQuery.Args>(new(id));
        var result = await mediator.SendAsync<GetSaleOrderByIdQuery.Args, GetSaleOrderByIdQuery.Result>(request);

        if (!result.IsSuccess) return Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
        if (result.Value?.SaleOrder is null) return Results.NotFound();

        return Results.Ok(new GetSaleOrderByIdResponseBody(result.Value.SaleOrder));
    }
}

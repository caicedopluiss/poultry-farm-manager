using System;
using System.Collections.Generic;
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

public class GetSaleOrdersByBatchIdEndpoint : IEndpoint
{
    public record GetSaleOrdersByBatchIdResponseBody(IEnumerable<SaleOrderDto> SaleOrders);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "batches", "{batchId:guid}", "sale-orders");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetSaleOrdersByBatchIdEndpoint))
            .WithTags("SaleOrders")
            .Produces<GetSaleOrdersByBatchIdResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid batchId,
        [FromServices] IAppRequestsMediator mediator)
    {
        var request = new AppRequest<GetSaleOrdersByBatchIdQuery.Args>(new(batchId));
        var result = await mediator.SendAsync<GetSaleOrdersByBatchIdQuery.Args, GetSaleOrdersByBatchIdQuery.Result>(request);

        return !result.IsSuccess
            ? Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors))
            : Results.Ok(new GetSaleOrdersByBatchIdResponseBody(result.Value!.SaleOrders));
    }
}

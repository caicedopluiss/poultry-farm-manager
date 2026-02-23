using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.Commands.Products;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.Products;

public sealed class AddProductStockEndpoint : IEndpoint
{
    public record AddProductStockRequestBody(Guid ProductVariantId, int Quantity);
    public record AddProductStockResponseBody(ProductDto UpdatedProduct);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "products", "{id:guid}", "add-stock");
        app.MapPost(route, HandleAsync)
            .WithName(nameof(AddProductStockEndpoint))
            .WithTags(nameof(Products))
            .Produces<AddProductStockResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] AddProductStockRequestBody body,
        [FromServices] IAppRequestsMediator mediator,
        CancellationToken cancellationToken)
    {
        var request = new AppRequest<AddProductStockCommand.Args>(new(id, body.ProductVariantId, body.Quantity));
        var result = await mediator.SendAsync<AddProductStockCommand.Args, AddProductStockCommand.Result>(request, cancellationToken);

        return !result.IsSuccess ?
            Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors)) :
            Results.Ok(new AddProductStockResponseBody(result.Value!.UpdatedProduct));
    }
}

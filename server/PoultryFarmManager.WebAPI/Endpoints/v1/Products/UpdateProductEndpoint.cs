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

public class UpdateProductEndpoint : IEndpoint
{
    public record UpdateProductRequestBody(UpdateProductDto UpdateProduct);
    public record UpdateProductResponseBody(ProductDto Product);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "products", "{id:guid}");
        app.MapPut(route, UpdateAsync)
            .WithName(nameof(UpdateProductEndpoint))
            .WithTags(nameof(Products))
            .Accepts<UpdateProductRequestBody>(MediaTypeNames.Application.Json)
            .Produces<UpdateProductResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] UpdateProductRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<UpdateProductCommand.Args>(new(id, body.UpdateProduct));
        try
        {
            var result = await mediator.SendAsync<UpdateProductCommand.Args, UpdateProductCommand.Result>(request, cancellationToken);

            return !result.IsSuccess ?
                Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors)) :
                Results.Ok(new UpdateProductResponseBody(result.Value!.UpdatedProduct));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound();
        }
    }
}

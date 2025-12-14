using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.Commands.ProductVariants;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.ProductVariants;

public class UpdateProductVariantEndpoint : IEndpoint
{
    public record UpdateProductVariantRequestBody(UpdateProductVariantDto UpdateProductVariant);
    public record UpdateProductVariantResponseBody(ProductVariantDto ProductVariant);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "product-variants", "{id:guid}");
        app.MapPut(route, UpdateAsync)
            .WithName(nameof(UpdateProductVariantEndpoint))
            .WithTags(nameof(ProductVariants))
            .Accepts<UpdateProductVariantRequestBody>(MediaTypeNames.Application.Json)
            .Produces<UpdateProductVariantResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] UpdateProductVariantRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<UpdateProductVariantCommand.Args>(new(id, body.UpdateProductVariant));
        try
        {
            var result = await mediator.SendAsync<UpdateProductVariantCommand.Args, UpdateProductVariantCommand.Result>(request, cancellationToken);

            return !result.IsSuccess ?
                Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors)) :
                Results.Ok(new UpdateProductVariantResponseBody(result.Value!.UpdatedProductVariant));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound();
        }
    }
}

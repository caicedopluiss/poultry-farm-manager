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

public class CreateProductVariantEndpoint : IEndpoint
{
    public record CreateProductVariantRequestBody(NewProductVariantDto NewProductVariant);
    public record CreateProductVariantResponseBody(ProductVariantDto ProductVariant);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "product-variants");
        app.MapPost(route, CreateAsync)
            .WithName(nameof(CreateProductVariantEndpoint))
            .WithTags(nameof(ProductVariants))
            .Accepts<CreateProductVariantRequestBody>(MediaTypeNames.Application.Json)
            .Produces<CreateProductVariantResponseBody>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> CreateAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] CreateProductVariantRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<CreateProductVariantCommand.Args>(new(body.NewProductVariant));
        var result = await mediator.SendAsync<CreateProductVariantCommand.Args, CreateProductVariantCommand.Result>(request, cancellationToken);

        return !result.IsSuccess ?
            Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors)) :
            Results.Created($"/api/v1/product-variants/{result.Value!.CreatedProductVariant.Id}", new CreateProductVariantResponseBody(result.Value!.CreatedProductVariant));
    }
}

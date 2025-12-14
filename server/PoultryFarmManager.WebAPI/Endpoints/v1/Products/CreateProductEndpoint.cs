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

public class CreateProductEndpoint : IEndpoint
{
    public record CreateProductRequestBody(NewProductDto NewProduct);
    public record CreateProductResponseBody(ProductDto Product);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "products");
        app.MapPost(route, CreateAsync)
            .WithName(nameof(CreateProductEndpoint))
            .WithTags(nameof(Products))
            .Accepts<CreateProductRequestBody>(MediaTypeNames.Application.Json)
            .Produces<CreateProductResponseBody>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> CreateAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] CreateProductRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<CreateProductCommand.Args>(new(body.NewProduct));
        var result = await mediator.SendAsync<CreateProductCommand.Args, CreateProductCommand.Result>(request, cancellationToken);

        return !result.IsSuccess ?
            Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors)) :
            Results.Created($"/api/v1/products/{result.Value!.CreatedProduct.Id}", new CreateProductResponseBody(result.Value!.CreatedProduct));
    }
}

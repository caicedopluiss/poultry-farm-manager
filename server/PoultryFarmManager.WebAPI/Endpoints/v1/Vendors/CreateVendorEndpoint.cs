using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.Commands.Vendors;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.Vendors;

public class CreateVendorEndpoint : IEndpoint
{
    public record CreateVendorRequestBody(NewVendorDto Vendor);
    public record CreateVendorResponseBody(VendorDto Vendor);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "vendors");
        app.MapPost(route, CreateAsync)
            .WithName(nameof(CreateVendorEndpoint))
            .WithTags(nameof(Vendors))
            .Accepts<CreateVendorRequestBody>(MediaTypeNames.Application.Json)
            .Produces<CreateVendorResponseBody>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> CreateAsync(
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] CreateVendorRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<CreateVendorCommand.Args>(new(body.Vendor));

        var result = await mediator.SendAsync<CreateVendorCommand.Args, CreateVendorCommand.Result>(request, cancellationToken);

        return !result.IsSuccess ?
            Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors)) :
            Results.Created(
                $"/api/v1/vendors/{result.Value!.CreatedVendor.Id}",
                new CreateVendorResponseBody(result.Value!.CreatedVendor));
    }
}

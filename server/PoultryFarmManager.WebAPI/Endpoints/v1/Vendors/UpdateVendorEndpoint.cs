using System;
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

public class UpdateVendorEndpoint : IEndpoint
{
    public record UpdateVendorRequestBody(UpdateVendorDto Vendor);
    public record UpdateVendorResponseBody(VendorDto Vendor);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "vendors", "{id:guid}");
        app.MapPut(route, UpdateAsync)
            .WithName(nameof(UpdateVendorEndpoint))
            .WithTags(nameof(Vendors))
            .Accepts<UpdateVendorRequestBody>(MediaTypeNames.Application.Json)
            .Produces<UpdateVendorResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound, MediaTypeNames.Application.Json)
            .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator,
        [FromBody] UpdateVendorRequestBody body,
        CancellationToken cancellationToken = default)
    {
        var request = new AppRequest<UpdateVendorCommand.Args>(new(id, body.Vendor));

        try
        {
            var result = await mediator.SendAsync<UpdateVendorCommand.Args, UpdateVendorCommand.Result>(request, cancellationToken);

            if (!result.IsSuccess)
            {
                return Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
            }

            return Results.Ok(new UpdateVendorResponseBody(result.Value!.UpdatedVendor));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new ErrorResponse(ex.Message, []));
        }
    }
}

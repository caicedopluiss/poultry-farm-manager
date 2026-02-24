using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PoultryFarmManager.Application.DTOs;
using PoultryFarmManager.Application.Queries.Vendors;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.WebAPI.Endpoints.v1.Vendors;

public class GetVendorByIdEndpoint : IEndpoint
{
    public record GetVendorByIdResponseBody(VendorDto Vendor);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "vendors", "{id:guid}");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetVendorByIdEndpoint))
            .WithTags(nameof(Vendors))
            .Produces<GetVendorByIdResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAsync(
        [FromRoute] Guid id,
        [FromServices] IAppRequestsMediator mediator
    )
    {
        var request = new AppRequest<GetVendorByIdQuery.Args>(new(id));
        var result = await mediator.SendAsync<GetVendorByIdQuery.Args, GetVendorByIdQuery.Result>(request);

        if (!result.IsSuccess) return Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors));
        if (result.Value?.Vendor is null) return Results.NotFound();

        return Results.Ok(new GetVendorByIdResponseBody(result.Value.Vendor));
    }
}

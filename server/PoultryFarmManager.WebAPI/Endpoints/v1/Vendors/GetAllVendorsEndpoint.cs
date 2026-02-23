using System.Collections.Generic;
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

public class GetAllVendorsEndpoint : IEndpoint
{
    public record GetAllVendorsResponseBody(IEnumerable<VendorDto> Vendors);

    public static void Map(IEndpointRouteBuilder app, string? prefix = null)
    {
        var route = Utils.BuildEndpointRoute(prefix, "v1", "vendors");
        app.MapGet(route, GetAsync)
            .WithName(nameof(GetAllVendorsEndpoint))
            .WithTags(nameof(Vendors))
            .Produces<GetAllVendorsResponseBody>(StatusCodes.Status200OK, MediaTypeNames.Application.Json);
    }

    private static async Task<IResult> GetAsync(
        [FromServices] IAppRequestsMediator mediator
    )
    {
        var request = new AppRequest<GetAllVendorsQuery.Args>(new());
        var result = await mediator.SendAsync<GetAllVendorsQuery.Args, GetAllVendorsQuery.Result>(request);

        return !result.IsSuccess
            ? Results.BadRequest(new ErrorResponse(result.Message, result.ValidationErrors))
            : Results.Ok(new GetAllVendorsResponseBody(result.Value!.Vendors));
    }
}

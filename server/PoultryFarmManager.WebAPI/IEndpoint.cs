using Microsoft.AspNetCore.Routing;

namespace PoultryFarmManager.WebAPI;

public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder app, string? prefix = null);
}

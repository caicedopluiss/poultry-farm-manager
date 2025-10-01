using System.Linq;
using Microsoft.AspNetCore.Routing;

namespace PoultryFarmManager.WebAPI;

internal static class Extensions
{
    internal static void MapEndpoints(this IEndpointRouteBuilder app, string? prefix = null)
    {
        foreach (var endpoint in typeof(Extensions).Assembly.GetTypes()
                     .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsAbstract))
        {
            var method = endpoint.GetMethod("Map", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            method?.Invoke(null, [app, prefix]);
        }
    }

    internal static void MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app, string? prefix = null) where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app, prefix);
    }
}

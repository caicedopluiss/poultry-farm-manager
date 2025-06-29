using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Operations.Commands;
using SharedLib.CQRS;

namespace PoultryFarmManager.Application;

public static class ApplicationServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        AddCommandHandlers(services);

        return services;
    }

    private static void AddCommandHandlers(IServiceCollection services)
    {
        services.AddScoped<IAppRequestHandler<CreateBroilerBatchCommand.Args, CreateBroilerBatchCommand.Result>, CreateBroilerBatchCommand.Handler>();
    }
}
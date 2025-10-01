using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.Batches;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application;

public static class ApplicationServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        AddCommandHandlers(services);
        services.AddSingleton<IAppRequestsMediator, AppRequestsMediator>();

        return services;
    }

    private static void AddCommandHandlers(IServiceCollection services)
    {
        services.AddScoped<IAppRequestHandler<CreateBatchCommand.Args, CreateBatchCommand.Result>, CreateBatchCommand.Handler>();
    }
}

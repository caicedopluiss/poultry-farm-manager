using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Operations.Commands;
using PoultryFarmManager.Application.Operations.Queries;
using SharedLib.CQRS;

namespace PoultryFarmManager.Application;

public static class ApplicationServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        AddCommandHandlers(services);
        AddQueryHandlers(services);

        return services;
    }

    private static void AddCommandHandlers(IServiceCollection services)
    {
        services.AddScoped<IAppRequestHandler<CreateBroilerBatchCommand.Args, CreateBroilerBatchCommand.Result>, CreateBroilerBatchCommand.Handler>();
        services.AddScoped<IAppRequestHandler<UpdateBroilerBatchCommand.Args, UpdateBroilerBatchCommand.Result>, UpdateBroilerBatchCommand.Handler>();
    }

    private static void AddQueryHandlers(IServiceCollection services)
    {
        services.AddScoped<IAppRequestHandler<ReadAllBroilerBatchQuery.Args, ReadAllBroilerBatchQuery.Result>, ReadAllBroilerBatchQuery.Handler>();
    }
}
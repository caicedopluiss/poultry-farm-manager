using System;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Operations.Commands;
using PoultryFarmManager.Application.Operations.Commands.ActivityDispatchers;
using PoultryFarmManager.Application.Operations.Queries;
using PoultryFarmManager.Application.Services;
using SharedLib.CQRS;

namespace PoultryFarmManager.Application;

public static class ApplicationServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        AddCommandHandlers(services);
        AddQueryHandlers(services);
        AddActivityCommandDispatchers(services); // Register all activity dispatchers

        return services;
    }

    private static void AddActivityCommandDispatchers(IServiceCollection services)
    {
        services.AddScoped<IActivityDispatcherFactory, ActivityDispatcherFactory>();

        services.AddScoped<IActivityDispatcher, WeightMeasurementActivityDispatcher>();
        services.AddScoped<IActivityDispatcher, MortalityActivityDispatcher>();
    }

    private static void AddCommandHandlers(IServiceCollection services)
    {
        services.AddScoped<IAppRequestHandler<CreateBroilerBatchCommand.Args, CreateBroilerBatchCommand.Result>, CreateBroilerBatchCommand.Handler>();
        services.AddScoped<IAppRequestHandler<UpdateBroilerBatchCommand.Args, UpdateBroilerBatchCommand.Result>, UpdateBroilerBatchCommand.Handler>();
        services.AddScoped<IAppRequestHandler<AddActivityCommand.Args, AddActivityCommand.Result>, AddActivityCommand.Handler>();
    }

    private static void AddQueryHandlers(IServiceCollection services)
    {
        services.AddScoped<IAppRequestHandler<ReadAllBroilerBatchQuery.Args, ReadAllBroilerBatchQuery.Result>, ReadAllBroilerBatchQuery.Handler>();
        services.AddScoped<IAppRequestHandler<GetBroilerBatchByIdQuery.Args, GetBroilerBatchByIdQuery.Result>, GetBroilerBatchByIdQuery.Handler>();
    }
}
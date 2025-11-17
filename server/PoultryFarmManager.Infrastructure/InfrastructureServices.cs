using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Repositories;
using PoultryFarmManager.Infrastructure.Repositories;

namespace PoultryFarmManager.Infrastructure;

public static class InfrastructureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddDbContext(services, configuration);

        AddRepositories(services);

        return services;
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("pfm");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("The application connection string is missing or empty in the configuration.");
        }
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IBatchesRepository, BatchesRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application;
using PoultryFarmManager.Application.Finances;
using PoultryFarmManager.Application.Operations.Repositories;
using PoultryFarmManager.Infrastructure.Repositories;

namespace PoultryFarmManager.Infrastructure;

public static class InfrastructureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection")));

        AddRepositories(services);

        return services;
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBroilerBatchRepository, BroilerBatchRepository>();
        services.AddScoped<IFinancesRepository, Finances>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
    }
}
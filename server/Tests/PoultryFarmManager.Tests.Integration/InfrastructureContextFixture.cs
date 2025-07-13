using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PoultryFarmManager.Application;
using PoultryFarmManager.Infrastructure;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PoultryFarmManager.Tests.Integration;

/// <summary>
/// This Fixture is meant to manage a database per each class test. So do not use is with the Collection attribute.
/// </summary>
public class InfrastructureContextFixture : IDisposable
{
    private readonly ServiceProvider serviceProvider;

    public InfrastructureContextFixture(string? name = null)
    {
        string dbName = $"TestDb_{name ?? Guid.NewGuid().ToString()}";
        var configData = new Dictionary<string, string?>
        {
            { "ConnectionStrings:SqlServerConnection", $"Server=localhost;Database={dbName};User Id=sa;Password=P@55word;TrustServerCertificate=True;"}
        };
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        services.AddApplicationServices();
        services.AddInfrastructureServices(configuration);
        services.AddDbContext<IntegrationTestsDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("SqlServerConnection"),
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure())
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        });

        serviceProvider = services.BuildServiceProvider();
        using var serviceScope = serviceProvider.CreateScope();
        using var dbContext = serviceScope.ServiceProvider.GetRequiredService<IntegrationTestsDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
    }

    public IServiceProvider CreateServicesScope()
    {
        var scope = serviceProvider.CreateScope();
        return scope.ServiceProvider;
    }
}
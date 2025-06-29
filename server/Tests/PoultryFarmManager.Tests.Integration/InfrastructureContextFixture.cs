using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PoultryFarmManager.Application;
using PoultryFarmManager.Infrastructure;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PoultryFarmManager.Tests.Integration;

public class InfrastructureContextFixture : IAsyncLifetime, IDisposable
{
    private readonly IServiceProvider serviceProvider;
    private readonly IServiceScope serviceScope;

    public IServiceProvider ServiceProvider => serviceScope.ServiceProvider;

    public InfrastructureContextFixture()
    {
        var services = new ServiceCollection();

        var configData = new Dictionary<string, string?>
        {
            { "ConnectionStrings:SqlServerConnection", "Server=localhost;Database=PoultryFarmManagerTestDb;User Id=sa;Password=P@55word;TrustServerCertificate=True;" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        services.AddApplicationServices();
        services.AddInfrastructureServices(configuration);

        serviceProvider = services.BuildServiceProvider();
        serviceScope = serviceProvider.CreateScope();
    }

    public void Dispose()
    {
        serviceScope.Dispose();
        if (serviceProvider is IDisposable disposable) disposable.Dispose();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public Task InitializeAsync()
    {
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        return Task.CompletedTask;
    }
}
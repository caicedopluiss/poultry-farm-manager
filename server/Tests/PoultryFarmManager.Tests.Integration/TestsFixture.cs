using System;
using PoultryFarmManager.Application;
using PoultryFarmManager.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using PoultryFarmManager.WebAPI;
using System.Linq;
using System.Net.Http;
using PoultryFarmManager.Core;

namespace PoultryFarmManager.Tests.Integration;

/// <summary>
/// This Fixture is meant to manage a database per each test class. So do not use it with the Collection attribute.
/// </summary>
public class TestsFixture : IDisposable
{
    private readonly WebApplicationFactory<Program> webApplicationFactory;
    private readonly IServiceScope serviceScope;

    private HttpClient? client;
    public HttpClient Client => client ??= webApplicationFactory.CreateClient();
    public IServiceProvider ServiceProvider => serviceScope.ServiceProvider;

    public TestsFixture()
    {
        var connectionString = $"Host=localhost;Port=5432;Database=PoultryFarmManager_{Guid.NewGuid()};Username=test-user;Password=1234";


        webApplicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:pfm", connectionString);
                builder.ConfigureServices((context, services) =>
                {
                    // Remove any possible real database context registration
                    var dbContextDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                    if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

                    services
                        .AddApplicationServices()
                        .AddInfrastructureServices(context.Configuration);

                    services.AddDbContext<TestsDbContext>(config =>
                    {
                        config.UseNpgsql(connectionString)
                            .EnableSensitiveDataLogging()
                            .EnableDetailedErrors();
                    });
                });
            });

        serviceScope = webApplicationFactory.Services.CreateScope();
        serviceScope.ServiceProvider.GetRequiredService<TestsDbContext>().Database.EnsureCreated();
    }

    public void Dispose()
    {
        serviceScope.ServiceProvider.GetRequiredService<TestsDbContext>().Database.EnsureDeleted();

        serviceScope.Dispose();
        webApplicationFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Creates a new IServiceScope. Use this method if you need a new scope for each test method.
    /// Must be disposed by the caller.
    /// </summary>
    /// <returns></returns>
    public IServiceScope CreateServicesScope() => webApplicationFactory.Services.CreateScope();

#pragma warning disable CA1822 // Mark members as static
    public T CreateRandomEntity<T>() where T : IDbEntity => TestEntityFactory.GetFactory<T>().CreateRandom();
#pragma warning restore CA1822 // Mark members as static
}

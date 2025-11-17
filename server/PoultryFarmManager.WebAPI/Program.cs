using System;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using PoultryFarmManager.Application;
using PoultryFarmManager.Infrastructure;
using PoultryFarmManager.WebAPI.Endpoints.v1.Batches;

namespace PoultryFarmManager.WebAPI;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options =>
        {
            // Disable Server header to avoid leaking server details.
            options.AddServerHeader = false;
        });

        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services
            .AddApplicationServices()
            .AddInfrastructureServices(builder.Configuration);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                builder => builder.AllowAnyOrigin()
                                  .AllowAnyMethod()
                                  .AllowAnyHeader());
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(
            config =>
        {
            config.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Poultry Farm Manager API",
                Version = "v1",
                Description = "API for managing poultry farm operations."
            });
        }
        );

        var app = builder.Build();

        if (args.Contains("migrate", StringComparer.InvariantCultureIgnoreCase))
        {
            ApplyDbMigrations.Run(app);
            Console.WriteLine("Migration completed.");

            if (args.Contains("--exit", StringComparer.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Exiting...");
                Environment.Exit(0);
                return;
            }

            Console.WriteLine("Starting WebAPI...");
        }

        app.UseSwagger();
        app.UseSwaggerUI(config =>
        {
            string endpointPrefix = string.Empty;
            config.SwaggerEndpoint($"{endpointPrefix}/swagger/v1/swagger.json", "v1");
            config.RoutePrefix = string.Empty;
        });

        string apiPrefix = "api";

        app.MapEndpoint<CreateBatchEndpoint>(apiPrefix);
        app.MapEndpoint<GetBatchesListEndpoint>(apiPrefix);
        app.MapEndpoint<GetBatchByIdEndpoint>(apiPrefix);

        app.UseCors("AllowAllOrigins");

        app.Run();
    }
}

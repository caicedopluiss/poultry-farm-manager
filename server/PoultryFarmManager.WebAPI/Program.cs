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
using PoultryFarmManager.WebAPI.Endpoints.v1;
using PoultryFarmManager.WebAPI.Endpoints.v1.Assets;
using PoultryFarmManager.WebAPI.Endpoints.v1.Batches;
using PoultryFarmManager.WebAPI.Endpoints.v1.Persons;
using PoultryFarmManager.WebAPI.Endpoints.v1.Products;
using PoultryFarmManager.WebAPI.Endpoints.v1.ProductVariants;
using PoultryFarmManager.WebAPI.Endpoints.v1.Transactions;
using PoultryFarmManager.WebAPI.Endpoints.v1.Vendors;

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

        // Batch endpoints
        app.MapEndpoint<CreateBatchEndpoint>(apiPrefix);
        app.MapEndpoint<GetBatchesListEndpoint>(apiPrefix);
        app.MapEndpoint<GetBatchByIdEndpoint>(apiPrefix);
        app.MapEndpoint<RegisterMortalityEndpoint>(apiPrefix);
        app.MapEndpoint<SwitchBatchStatusEndpoint>(apiPrefix);
        app.MapEndpoint<RegisterProductConsumptionEndpoint>(apiPrefix);
        app.MapEndpoint<RegisterWeightMeasurementEndpoint>(apiPrefix);
        app.MapEndpoint<UpdateBatchNameEndpoint>(apiPrefix);

        // Asset endpoints
        app.MapEndpoint<CreateAssetEndpoint>(apiPrefix);
        app.MapEndpoint<GetAllAssetsEndpoint>(apiPrefix);
        app.MapEndpoint<GetAssetByIdEndpoint>(apiPrefix);
        app.MapEndpoint<UpdateAssetEndpoint>(apiPrefix);

        // Product endpoints
        app.MapEndpoint<CreateProductEndpoint>(apiPrefix);
        app.MapEndpoint<GetAllProductsEndpoint>(apiPrefix);
        app.MapEndpoint<GetProductByIdEndpoint>(apiPrefix);
        app.MapEndpoint<UpdateProductEndpoint>(apiPrefix);

        // ProductVariant endpoints
        app.MapEndpoint<CreateProductVariantEndpoint>(apiPrefix);
        app.MapEndpoint<GetAllProductVariantsEndpoint>(apiPrefix);
        app.MapEndpoint<GetProductVariantByIdEndpoint>(apiPrefix);
        app.MapEndpoint<GetProductVariantsByProductIdEndpoint>(apiPrefix);
        app.MapEndpoint<UpdateProductVariantEndpoint>(apiPrefix);

        // Transaction endpoints
        app.MapEndpoint<CreateTransactionEndpoint>(apiPrefix);

        // Person endpoints
        app.MapEndpoint<CreatePersonEndpoint>(apiPrefix);
        app.MapEndpoint<GetAllPersonsEndpoint>(apiPrefix);
        app.MapEndpoint<GetPersonByIdEndpoint>(apiPrefix);
        app.MapEndpoint<UpdatePersonEndpoint>(apiPrefix);

        // Vendor endpoints
        app.MapEndpoint<CreateVendorEndpoint>(apiPrefix);
        app.MapEndpoint<GetAllVendorsEndpoint>(apiPrefix);
        app.MapEndpoint<GetVendorByIdEndpoint>(apiPrefix);
        app.MapEndpoint<UpdateVendorEndpoint>(apiPrefix);

        app.UseCors("AllowAllOrigins");

        app.Run();
    }
}

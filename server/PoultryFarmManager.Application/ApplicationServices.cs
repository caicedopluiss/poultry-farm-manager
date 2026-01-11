using System;
using Microsoft.Extensions.DependencyInjection;
using PoultryFarmManager.Application.Commands.Assets;
using PoultryFarmManager.Application.Commands.Batches;
using PoultryFarmManager.Application.Commands.Products;
using PoultryFarmManager.Application.Commands.ProductVariants;
using PoultryFarmManager.Application.Queries.Assets;
using PoultryFarmManager.Application.Queries.Batches;
using PoultryFarmManager.Application.Queries.Products;
using PoultryFarmManager.Application.Queries.ProductVariants;
using PoultryFarmManager.Application.Shared.CQRS;

namespace PoultryFarmManager.Application;

public static class ApplicationServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        AddCommandHandlers(services);
        AddQueryHandlers(services);
        services.AddSingleton<IAppRequestsMediator, AppRequestsMediator>();

        return services;
    }

    private static void AddCommandHandlers(IServiceCollection services)
    {
        // Batches
        services.AddScoped<IAppRequestHandler<CreateBatchCommand.Args, CreateBatchCommand.Result>, CreateBatchCommand.Handler>();
        services.AddScoped<IAppRequestHandler<RegisterMortalityCommand.Args, RegisterMortalityCommand.Result>, RegisterMortalityCommand.Handler>();
        services.AddScoped<IAppRequestHandler<SwitchBatchStatusCommand.Args, SwitchBatchStatusCommand.Result>, SwitchBatchStatusCommand.Handler>();
        services.AddScoped<IAppRequestHandler<RegisterProductConsumptionCommand.Args, RegisterProductConsumptionCommand.Result>, RegisterProductConsumptionCommand.Handler>();
        services.AddScoped<IAppRequestHandler<UpdateBatchNameCommand.Args, UpdateBatchNameCommand.Result>, UpdateBatchNameCommand.Handler>();

        // Assets
        services.AddScoped<IAppRequestHandler<CreateAssetCommand.Args, CreateAssetCommand.Result>, CreateAssetCommand.Handler>();
        services.AddScoped<IAppRequestHandler<UpdateAssetCommand.Args, UpdateAssetCommand.Result>, UpdateAssetCommand.Handler>();

        // Products
        services.AddScoped<IAppRequestHandler<CreateProductCommand.Args, CreateProductCommand.Result>, CreateProductCommand.Handler>();
        services.AddScoped<IAppRequestHandler<UpdateProductCommand.Args, UpdateProductCommand.Result>, UpdateProductCommand.Handler>();

        // Product Variants
        services.AddScoped<IAppRequestHandler<CreateProductVariantCommand.Args, CreateProductVariantCommand.Result>, CreateProductVariantCommand.Handler>();
        services.AddScoped<IAppRequestHandler<UpdateProductVariantCommand.Args, UpdateProductVariantCommand.Result>, UpdateProductVariantCommand.Handler>();
    }

    private static void AddQueryHandlers(IServiceCollection services)
    {
        // Batches
        services.AddScoped<IAppRequestHandler<GetBatchesListQuery.Args, GetBatchesListQuery.Result>, GetBatchesListQuery.Handler>();
        services.AddScoped<IAppRequestHandler<GetBatchByIdQuery.Args, GetBatchByIdQuery.Result>, GetBatchByIdQuery.Handler>();

        // Assets
        services.AddScoped<IAppRequestHandler<GetAllAssetsQuery.Args, GetAllAssetsQuery.Result>, GetAllAssetsQuery.Handler>();
        services.AddScoped<IAppRequestHandler<GetAssetByIdQuery.Args, GetAssetByIdQuery.Result>, GetAssetByIdQuery.Handler>();

        // Products
        services.AddScoped<IAppRequestHandler<GetAllProductsQuery.Args, GetAllProductsQuery.Result>, GetAllProductsQuery.Handler>();
        services.AddScoped<IAppRequestHandler<GetProductByIdQuery.Args, GetProductByIdQuery.Result>, GetProductByIdQuery.Handler>();

        // Product Variants
        services.AddScoped<IAppRequestHandler<GetAllProductVariantsQuery.Args, GetAllProductVariantsQuery.Result>, GetAllProductVariantsQuery.Handler>();
        services.AddScoped<IAppRequestHandler<GetProductVariantByIdQuery.Args, GetProductVariantByIdQuery.Result>, GetProductVariantByIdQuery.Handler>();
        services.AddScoped<IAppRequestHandler<GetProductVariantsByProductIdQuery.Args, GetProductVariantsByProductIdQuery.Result>, GetProductVariantsByProductIdQuery.Handler>();
    }
}

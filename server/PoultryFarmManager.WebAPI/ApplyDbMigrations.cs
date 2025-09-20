using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PoultryFarmManager.Infrastructure;

namespace PoultryFarmManager.WebAPI
{
    internal class ApplyDbMigrations
    {
        internal static void Run(IHost application)
        {
            Console.WriteLine("Applying database migrations...");
            try
            {
                using var scope = application.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
                Console.WriteLine("Database migrated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database migration failed: {ex.Message}");
                throw;
            }
        }
    }
}

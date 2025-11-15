using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
                // GrantPermissionsToAppUser(dbContext, scope.ServiceProvider.GetRequiredService<IConfiguration>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database migration failed: {ex.Message}");
                throw;
            }
        }

        private static void GrantPermissionsToAppUser(AppDbContext dbContext, IConfiguration configuration)
        {
            Console.WriteLine("Trying to Grant permissions to application user...");
            var dbSecretsJson = configuration.GetValue<string>("DB_SECRETS");
            if (string.IsNullOrEmpty(dbSecretsJson)) return;

            try
            {
                using var document = JsonDocument.Parse(dbSecretsJson);
                if (!document.RootElement.TryGetProperty("app_user", out var appUserElement)) return;

                var appUser = appUserElement.GetString();
                if (string.IsNullOrEmpty(appUser)) return;

                Console.WriteLine($"Granting permissions to {appUser}...");
                dbContext.Database.ExecuteSqlRaw("GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE ON ALL TABLES IN SCHEMA public TO \"{0}\";", appUser);

                Console.WriteLine($"Permissions granted to {appUser}.");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Warning: Could not parse secrets JSON: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Warning: Database update failed while granting permissions: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not grant permissions: {ex.Message}");
            }
        }
    }
}

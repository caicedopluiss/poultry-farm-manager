using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
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
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                using var dbContext = GetDbContextWithAdminCredentials(scope, configuration);

                dbContext.Database.Migrate();
                Console.WriteLine("Database migrated successfully.");

                // GrantPermissionsToAppUser(dbContext, configuration);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database migration failed: {ex.Message}");
                throw;
            }
        }

        private static AppDbContext GetDbContextWithAdminCredentials(IServiceScope scope, IConfiguration configuration)
        {
            var defaultDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var dbSecretsJson = configuration.GetValue<string>("DB_SECRETS");

            // If no DB_SECRETS, use the default connection string
            if (string.IsNullOrEmpty(dbSecretsJson))
            {
                Console.WriteLine("No Database secrets found. Using default connection string for migrations.");
                return defaultDbContext;
            }

            try
            {
                using var document = JsonDocument.Parse(dbSecretsJson);

                if (!document.RootElement.TryGetProperty("admin_user", out var adminUserElement))
                {
                    Console.WriteLine("Admin user not found in database secrets. Using default connection string.");
                    return defaultDbContext;
                }

                var adminUser = adminUserElement.GetString();

                if (string.IsNullOrEmpty(adminUser))
                {
                    Console.WriteLine("Admin user is empty. Using default connection string.");
                    return defaultDbContext;
                }

                var originalConnectionString = defaultDbContext.Database.GetConnectionString();

                var builderOriginal = new NpgsqlConnectionStringBuilder(originalConnectionString);
                var originalUser = builderOriginal.Username;

                if (adminUser == originalUser)
                {
                    Console.WriteLine("Application is using admin user. Using default connection string. Is this intended?");
                    return defaultDbContext;
                }

                if (!document.RootElement.TryGetProperty("admin_user_password", out var adminPasswordElement))
                {
                    Console.WriteLine("Admin password not found in database secrets. Using default connection string.");
                    return defaultDbContext;
                }

                var adminPassword = adminPasswordElement.GetString();

                if (string.IsNullOrEmpty(adminPassword))
                {
                    Console.WriteLine("Admin user password is empty. Using default connection string.");
                    return defaultDbContext;
                }

                // Parse and modify the connection string to use admin credentials
                var builder = new NpgsqlConnectionStringBuilder(originalConnectionString)
                {
                    Username = adminUser,
                    Password = adminPassword
                };

                var adminConnectionString = builder.ConnectionString;
                Console.WriteLine($"Using admin user from database secrets for migrations.");

                // Create a new DbContext with admin connection string
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseNpgsql(adminConnectionString);

                return new AppDbContext(optionsBuilder.Options);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Warning: Could not parse database secrets JSON: {ex.Message}");
                Console.WriteLine("Using default connection string.");
                return defaultDbContext;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not create admin DbContext: {ex.Message}");
                Console.WriteLine("Using default connection string.");
                return defaultDbContext;
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

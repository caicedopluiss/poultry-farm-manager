using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Infrastructure;

namespace PoultryFarmManager.Tests.Integration;

internal class TestsDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    internal async Task ClearDbAsync()
    {
        // Clear in correct order respecting foreign key constraints
        // Finance tables: Transactions reference Vendors, Vendors reference Persons
        await Transactions.ExecuteDeleteAsync();
        await Vendors.ExecuteDeleteAsync();
        await Persons.ExecuteDeleteAsync();

        // Clear batch and activities
        await MortalityRegistrationActivities.ExecuteDeleteAsync();
        await StatusSwitchActivities.ExecuteDeleteAsync();
        await WeightMeasurementActivities.ExecuteDeleteAsync();
        await Batches.ExecuteDeleteAsync();

        // Clear inventory
        // AssetStates cascade delete with Assets, so delete Assets first (cascade deletes AssetStates)
        // ProductVariants reference Products, so delete ProductVariants first
        await Assets.ExecuteDeleteAsync();  // This cascade deletes AssetStates
        await ProductVariants.ExecuteDeleteAsync();
        await Products.ExecuteDeleteAsync();

        // Clear the EF Core change tracker to prevent stale entity references
        // ExecuteDeleteAsync bypasses change tracking, so we need to manually clear tracked entities
        ChangeTracker.Clear();
    }
}

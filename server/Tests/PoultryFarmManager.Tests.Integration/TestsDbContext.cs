using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Infrastructure;

namespace PoultryFarmManager.Tests.Integration;

internal class TestsDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    internal Task ClearDbAsync()
    {
        // Clear Finance tables first (due to foreign key dependencies)
        Transactions.RemoveRange(Transactions);
        Vendors.RemoveRange(Vendors);
        Persons.RemoveRange(Persons);

        // Clear batch and activities
        Batches.RemoveRange(Batches);
        MortalityRegistrationActivities.RemoveRange(MortalityRegistrationActivities);
        StatusSwitchActivities.RemoveRange(StatusSwitchActivities);
        WeightMeasurementActivities.RemoveRange(WeightMeasurementActivities);

        // Clear assets and inventory
        AssetStates.RemoveRange(AssetStates);
        Assets.RemoveRange(Assets);
        ProductVariants.RemoveRange(ProductVariants);
        Products.RemoveRange(Products);

        return SaveChangesAsync();
    }
}

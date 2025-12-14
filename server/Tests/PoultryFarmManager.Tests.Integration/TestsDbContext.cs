using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Infrastructure;

namespace PoultryFarmManager.Tests.Integration;

internal class TestsDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    internal Task ClearDbAsync()
    {
        Batches.RemoveRange(Batches);
        MortalityRegistrationActivities.RemoveRange(MortalityRegistrationActivities);
        StatusSwitchActivities.RemoveRange(StatusSwitchActivities);
        AssetStates.RemoveRange(AssetStates);
        Assets.RemoveRange(Assets);
        ProductVariants.RemoveRange(ProductVariants);
        Products.RemoveRange(Products);
        return SaveChangesAsync();
    }
}

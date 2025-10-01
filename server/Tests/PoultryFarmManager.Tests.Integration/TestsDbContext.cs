using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Infrastructure;

namespace PoultryFarmManager.Tests.Integration;

internal class TestsDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    internal Task ClearDbAsync()
    {
        Batches.RemoveRange(Batches);
        return SaveChangesAsync();
    }
}

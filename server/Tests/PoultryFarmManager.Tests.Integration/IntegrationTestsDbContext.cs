using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Infrastructure;

namespace PoultryFarmManager.Tests.Integration
{
    public class IntegrationTestsDbContext(DbContextOptions<ApplicationDbContext> options) : ApplicationDbContext(options)
    {
        internal Task ClearDatabaseAsync()
        {
            BroilerBatches.RemoveRange(BroilerBatches);
            FinancialTransactions.RemoveRange(FinancialTransactions);
            FinancialEntities.RemoveRange(FinancialEntities);
            // Add other DbSet clearings as needed

            return SaveChangesAsync();
        }
    }
}
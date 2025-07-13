using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Core;
using PoultryFarmManager.Core.Finances.Models;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<BroilerBatch> BroilerBatches { get; set; }
    public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
    public DbSet<FinancialEntity> FinancialEntities { get; set; }
    public DbSet<Activity> Activities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IDbEntity>())
        {
            var currentDate = DateTime.UtcNow;

            entry.Entity.ModifiedAt = currentDate;

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = currentDate;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Core.Models;

namespace PoultryFarmManager.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Batch> Batches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Batch>(entity =>
        {
            entity.ToTable(nameof(Batches));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.StartDate);
            entity.Property(e => e.Breed).HasMaxLength(100);
            entity.Property(e => e.MaleCount);
            entity.Property(e => e.FemaleCount);
            entity.Property(e => e.UnsexedCount);
            entity.Property(e => e.InitialPopulation);
            entity.Property(e => e.Status);
        });
    }
}

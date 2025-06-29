using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public DbSet<BroilerBatch> BroilerBatches { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<BroilerBatch>(entity =>
        {
            entity.ToTable(nameof(ApplicationDbContext.BroilerBatches));

            entity.HasKey(b => b.Id);

            entity.Property(b => b.Id)
                .ValueGeneratedOnAdd();

            entity.Property(b => b.BatchName)
                .HasMaxLength(100);

            entity.Property(b => b.InitialPopulation)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(b => b.CurrentPopulation)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(b => b.Status)
                .IsRequired();

            entity.Property(b => b.Breed)
                .HasMaxLength(50)
                .IsRequired(false);

            entity.Property(b => b.StartDate)
                .IsRequired(false);

            entity.Property(b => b.ProcessingStartDate)
                .IsRequired(false);

            entity.Property(b => b.ProcessingEndDate)
                .IsRequired(false);

            entity.Property(b => b.Notes)
                .HasMaxLength(500)
                .IsRequired(false);

            entity.Property(b => b.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(b => b.ModifiedAt)
                .IsRequired(false);
        });
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Infrastructure.DbEntityConfigurations;

public class BroilerBatchEntityConfiguration : IEntityTypeConfiguration<BroilerBatch>
{
    public void Configure(EntityTypeBuilder<BroilerBatch> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.BroilerBatches));

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedOnAdd();

        builder.Property(b => b.BatchName)
            .HasMaxLength(100);

        builder.Property(b => b.InitialPopulation)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.CurrentPopulation)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.Status)
            .IsRequired();

        builder.Property(b => b.Breed)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(b => b.StartDate)
            .IsRequired(false);

        builder.Property(b => b.ProcessingStartDate)
            .IsRequired(false);

        builder.Property(b => b.ProcessingEndDate)
            .IsRequired(false);

        builder.Property(b => b.Notes)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(b => b.ModifiedAt)
            .IsRequired(false);
    }
}
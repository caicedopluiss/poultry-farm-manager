using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoultryFarmManager.Core.Operations.Models;

namespace PoultryFarmManager.Infrastructure.DbEntityConfigurations;

public class ActivityEntityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.Activities));

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .IsRequired()
            .HasColumnType("uniqueidentifier");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BroilerBatchId)
            .IsRequired()
            .HasColumnType("uniqueidentifier");

        builder.HasIndex(x => x.BroilerBatchId);
        builder.HasOne(x => x.BroilerBatch)
            .WithMany()
            .HasForeignKey(x => x.BroilerBatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Description)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.Value)
            .IsRequired(false)
            .HasPrecision(10, 2);

        builder.Property(x => x.Unit)
            .IsRequired(false)
            .HasMaxLength(10);

        builder.Property(x => x.Type)
            .IsRequired();
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoultryFarmManager.Core.Finances.Models;

namespace PoultryFarmManager.Infrastructure.DbEntityConfigurations;

public class FinancialEntityEntityConfiguration : IEntityTypeConfiguration<FinancialEntity>
{
    public void Configure(EntityTypeBuilder<FinancialEntity> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.FinancialEntities));
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .ValueGeneratedOnAdd();

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(f => f.Name)
            .IsUnique();

        builder.Property(f => f.ContactPhoneNumber)
            .IsRequired(false)
            .HasMaxLength(15);

        builder.Property(f => f.Type)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(f => f.ModifiedAt)
            .IsRequired(false);

    }
}
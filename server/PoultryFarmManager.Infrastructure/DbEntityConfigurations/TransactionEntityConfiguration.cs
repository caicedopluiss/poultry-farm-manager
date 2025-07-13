using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PoultryFarmManager.Core.Finances.Models;

namespace PoultryFarmManager.Infrastructure.DbEntityConfigurations;

public class TransactionEntityConfiguration : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.ToTable(nameof(ApplicationDbContext.FinancialTransactions));
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        builder.Property(t => t.TransactionDate)
            .IsRequired()
            .HasColumnType("datetime");
        builder.HasIndex(t => t.TransactionDate);


        builder.Property(t => t.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.PaidAmount)
            .IsRequired(false)
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.Category)
            .IsRequired();

        builder.Property(t => t.Type)
            .IsRequired();

        builder.Property(t => t.DueDate)
            .IsRequired(false)
            .HasColumnType("datetime");

        builder.Property(t => t.FinancialEntityId)
            .IsRequired()
            .HasColumnType("uniqueidentifier");
        builder.HasIndex(t => t.FinancialEntityId);

        builder.HasOne(t => t.FinancialEntity)
            .WithMany()
            .HasForeignKey(t => t.FinancialEntityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.ModifiedAt)
            .IsRequired(false);

        builder.Property(t => t.Notes)
            .IsRequired(false)
            .HasMaxLength(500);
    }
}
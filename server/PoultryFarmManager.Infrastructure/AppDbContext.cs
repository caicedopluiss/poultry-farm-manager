using System;
using Microsoft.EntityFrameworkCore;
using PoultryFarmManager.Core.Models;
using PoultryFarmManager.Core.Models.BatchActivities;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Batch> Batches { get; set; }
    public DbSet<MortalityRegistrationBatchActivity> MortalityRegistrationActivities { get; set; }
    public DbSet<StatusSwitchBatchActivity> StatusSwitchActivities { get; set; }
    public DbSet<ProductConsumptionBatchActivity> ProductConsumptionActivities { get; set; }
    public DbSet<WeightMeasurementBatchActivity> WeightMeasurementActivities { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetState> AssetStates { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }

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
            entity.Property(e => e.Shed).HasMaxLength(100);
            entity.Property(e => e.MaleCount);
            entity.Property(e => e.FemaleCount);
            entity.Property(e => e.UnsexedCount);
            entity.Property(e => e.InitialPopulation);
            entity.Property(e => e.Status);
        });

        modelBuilder.Entity<MortalityRegistrationBatchActivity>(entity =>
        {
            entity.ToTable(nameof(MortalityRegistrationActivities));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.BatchId).IsRequired();
            entity.HasIndex(e => e.BatchId);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.Property(e => e.NumberOfDeaths).IsRequired();
            entity.Property(e => e.Sex).IsRequired();

            entity.HasOne(e => e.Batch)
                .WithMany()
                .HasForeignKey(e => e.BatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StatusSwitchBatchActivity>(entity =>
        {
            entity.ToTable(nameof(StatusSwitchActivities));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.BatchId).IsRequired();
            entity.HasIndex(e => e.BatchId);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.Property(e => e.NewStatus).IsRequired();

            entity.HasOne(e => e.Batch)
                .WithMany()
                .HasForeignKey(e => e.BatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductConsumptionBatchActivity>(entity =>
        {
            entity.ToTable(nameof(ProductConsumptionActivities));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.BatchId).IsRequired();
            entity.HasIndex(e => e.BatchId);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.Stock).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.UnitOfMeasure).IsRequired();

            entity.HasOne(e => e.Batch)
                .WithMany()
                .HasForeignKey(e => e.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WeightMeasurementBatchActivity>(entity =>
        {
            entity.ToTable(nameof(WeightMeasurementActivities));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.BatchId).IsRequired();
            entity.HasIndex(e => e.BatchId);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.Property(e => e.AverageWeight).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.SampleSize).IsRequired();
            entity.Property(e => e.UnitOfMeasure).IsRequired();

            entity.HasOne(e => e.Batch)
                .WithMany()
                .HasForeignKey(e => e.BatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable(nameof(Assets));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(500);

        });

        modelBuilder.Entity<AssetState>(entity =>
        {
            entity.ToTable(nameof(AssetStates));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.AssetId).IsRequired();
            entity.HasOne(e => e.Asset)
                .WithMany(x => x.States)
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable(nameof(Products));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => new { e.Name, e.Manufacturer, e.UnitOfMeasure }).IsUnique();
            entity.Property(e => e.Manufacturer).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UnitOfMeasure).IsRequired();
            entity.Property(e => e.Stock).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.ToTable(nameof(ProductVariants));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => new { e.ProductId, e.Name }).IsUnique();
            entity.Property(e => e.UnitOfMeasure).IsRequired();
            entity.Property(e => e.Stock).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.Product)
                .WithMany(x => x.Variants)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}

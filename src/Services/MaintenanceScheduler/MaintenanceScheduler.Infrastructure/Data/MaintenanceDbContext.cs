using Microsoft.EntityFrameworkCore;
using MaintenanceScheduler.Domain.Entities;
using System;

namespace MaintenanceScheduler.Infrastructure.Data
{
    public class MaintenanceDbContext : DbContext
    {
        public MaintenanceDbContext(DbContextOptions<MaintenanceDbContext> options)
            : base(options)
        {
        }

        public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
        public DbSet<MaintenanceHistory> MaintenanceHistories { get; set; }
        public DbSet<WarrantyInfo> WarrantyInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // MaintenanceSchedule Configuration
            modelBuilder.Entity<MaintenanceSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.AssetName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.AssignedTo).HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.EstimatedCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ActualCost).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.AssetId);
                entity.HasIndex(e => e.ScheduledDate);
                entity.HasIndex(e => e.Status);
            });

            // MaintenanceHistory Configuration
            modelBuilder.Entity<MaintenanceHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.PartsReplaced).HasMaxLength(1000);
                entity.Property(e => e.TechnicianName).HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.AttachmentUrls).HasMaxLength(2000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.AssetId);
                entity.HasIndex(e => e.MaintenanceDate);

                entity.HasOne(e => e.MaintenanceSchedule)
                    .WithMany(s => s.MaintenanceHistories)
                    .HasForeignKey(e => e.MaintenanceScheduleId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // WarrantyInfo Configuration
            modelBuilder.Entity<WarrantyInfo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.AssetName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.WarrantyProvider).IsRequired().HasMaxLength(200);
                entity.Property(e => e.WarrantyType).HasMaxLength(100);
                entity.Property(e => e.CoverageDetails).HasMaxLength(2000);
                entity.Property(e => e.ContactPerson).HasMaxLength(200);
                entity.Property(e => e.ContactEmail).HasMaxLength(200);
                entity.Property(e => e.ContactPhone).HasMaxLength(50);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.WarrantyCost).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.AssetId);
                entity.HasIndex(e => e.ExpiryDate);
                entity.HasIndex(e => e.IsActive);
            });
        }
    }
}
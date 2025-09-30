using AssetRegistry.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetRegistry.Infrastructure.Persistence;

public class AssetRegistryDbContext : DbContext
{
    public AssetRegistryDbContext(DbContextOptions<AssetRegistryDbContext> options) : base(options) { }

    public DbSet<Asset> Assets => Set<Asset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("assets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).IsRequired().HasMaxLength(64);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Name).IsRequired().HasMaxLength(256);
            entity.Property(x => x.Category).HasMaxLength(128);
            entity.Property(x => x.Location).HasMaxLength(128);
            entity.Property(x => x.PurchasedAt).IsRequired();
        });
    }
}
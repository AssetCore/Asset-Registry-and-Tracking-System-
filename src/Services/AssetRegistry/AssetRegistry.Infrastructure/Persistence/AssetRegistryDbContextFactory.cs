using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace AssetRegistry.Infrastructure.Persistence
{
    public class AssetRegistryDbContextFactory : IDesignTimeDbContextFactory<AssetRegistryDbContext>
    {
        public AssetRegistryDbContext CreateDbContext(string[] args)
        {
            // Find the API project folder relative to current working directory
            var basePath = Directory.GetCurrentDirectory();

            // Walk up until we find the API project folder
            var apiProjectPath = Path.Combine(basePath, "../AssetRegistry.Api");

            // Fix for design-time path resolution
            if (!Directory.Exists(apiProjectPath))
            {
                apiProjectPath = Path.Combine(basePath, "../../AssetRegistry.Api");
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var cs = config.GetConnectionString("AssetRegistryDb")
                     ?? Environment.GetEnvironmentVariable("ASSETREGISTRY_CS")
                     ?? "Server=localhost;Database=AssetRegistry;Trusted_Connection=True;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<AssetRegistryDbContext>();
            optionsBuilder.UseSqlServer(cs);

            return new AssetRegistryDbContext(optionsBuilder.Options);
        }
    }
}

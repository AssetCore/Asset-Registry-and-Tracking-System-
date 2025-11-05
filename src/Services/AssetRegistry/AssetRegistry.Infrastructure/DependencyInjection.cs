using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AssetRegistry.Infrastructure.Persistence;
using AssetRegistry.Infrastructure.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddAssetRegistryInfrastructure(this IServiceCollection services, string cs)
    {
        services.AddDbContext<AssetRegistryDbContext>(opt => opt.UseSqlServer(cs));
        services.AddScoped<IAssetRepository, AssetRepository>();
        return services;
    }
}
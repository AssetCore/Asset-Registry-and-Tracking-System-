using AssetRegistry.Application.Interfaces;
using AssetRegistry.Infrastructure.Configuration;
using AssetRegistry.Infrastructure.Messaging;
using AssetRegistry.Infrastructure.Persistence;
using AssetRegistry.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddAssetRegistryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AssetRegistryDbContext>(opt =>
            opt.UseSqlServer(connectionString));

        services.AddScoped<IAssetRepository, AssetRepository>();

        services.Configure<RabbitMqSettings>(
            configuration.GetSection(RabbitMqSettings.SectionName));

        services.AddSingleton<INotificationEventPublisher, RabbitMqNotificationEventPublisher>();

        return services;
    }
}
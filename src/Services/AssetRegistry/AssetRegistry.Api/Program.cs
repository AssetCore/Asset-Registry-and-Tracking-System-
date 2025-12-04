using AssetRegistry.Application.Assets;
using AssetRegistry.Application.Interfaces;
using AssetRegistry.Infrastructure.Configuration;
using AssetRegistry.Infrastructure.Messaging;
using AssetRegistry.Infrastructure.Persistence;
using AssetRegistry.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Asset Registry API",
        Version = "v1",
        Description = "CRUD API for assets"
    });
});

builder.Services.AddDbContext<AssetRegistryDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("AssetRegistryDb")));

builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IAssetService, AssetService>();

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection(RabbitMqSettings.SectionName));

builder.Services.AddSingleton<INotificationEventPublisher, RabbitMqNotificationEventPublisher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Asset Registry API v1");
        c.RoutePrefix = "swagger"; 
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();   

app.Run();

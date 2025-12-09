using AssetRegistry.Application.Assets;
using AssetRegistry.Application.Interfaces;
using AssetRegistry.Infrastructure.Configuration;
using AssetRegistry.Infrastructure.Messaging;
using AssetRegistry.Infrastructure.Persistence;
using AssetRegistry.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.OpenSearch;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with OpenSearch
var openSearchUri = builder.Configuration["OpenSearch:Uri"];
var openSearchUsername = builder.Configuration["OpenSearch:Username"];
var openSearchPassword = builder.Configuration["OpenSearch:Password"];
var indexFormat = builder.Configuration["OpenSearch:IndexFormat"] ?? "asset-registry-logs-{0:yyyy.MM.dd}";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/asset-registry-log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri(openSearchUri ?? "http://3.150.64.215:9200"))
    {
        IndexFormat = indexFormat,
        ModifyConnectionSettings = c => c
            .BasicAuthentication(openSearchUsername ?? "admin", openSearchPassword ?? "MyStrongPassword123!")
            .ServerCertificateValidationCallback((o, c, ch, er) => true)
    })
    .CreateLogger();

builder.Host.UseSerilog();

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

Log.Information("Starting Asset Registry API");

app.Run();

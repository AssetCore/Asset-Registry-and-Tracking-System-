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

// Enable Serilog self-logging to see internal errors
Serilog.Debugging.SelfLog.Enable(Console.Error);

// Configure Serilog with OpenSearch from configuration
var openSearchUri = builder.Configuration["OpenSearch:Uri"] ?? "http://3.150.64.215:9200";
var openSearchUsername = builder.Configuration["OpenSearch:Username"] ?? "admin";
var openSearchPassword = builder.Configuration["OpenSearch:Password"] ?? "MyStrongPassword123!";
var indexFormat = builder.Configuration["OpenSearch:IndexFormat"] ?? "asset-registry-logs-{0:yyyy.MM.dd}";

var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console() // Keep this for debugging
    .WriteTo.File("logs/asset-registry-log-.txt", rollingInterval: RollingInterval.Day);

// Only add OpenSearch sink if connection test passes
var enableOpenSearch = builder.Configuration.GetValue<bool>("OpenSearch:Enabled", false);
if (enableOpenSearch)
{
    loggerConfig.WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri(openSearchUri))
    {
        IndexFormat = indexFormat,
        ModifyConnectionSettings = c => c
            .BasicAuthentication(openSearchUsername, openSearchPassword)
            .ServerCertificateValidationCallback((o, c, ch, er) => true)
            .RequestTimeout(TimeSpan.FromSeconds(30))
            .PingTimeout(TimeSpan.FromSeconds(5))
            .SniffOnStartup(false)
            .SniffOnConnectionFault(false)
            .DisableDirectStreaming(false),
        FailureCallback = (e) =>
        {
            Console.WriteLine($"[OPENSEARCH ERROR] Failed to send log to OpenSearch: {e.Exception?.Message}");
            Console.WriteLine($"[OPENSEARCH ERROR] Details: {e.MessageTemplate}");
        },
        AutoRegisterTemplate = true
    });
}

Log.Logger = loggerConfig.CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

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

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AssetRegistryDbContext>();
    try
    {
        db.Database.Migrate();
        Log.Information("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error applying migrations");
        throw;
    }
}

// Pipeline
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

// Enable CORS
app.UseCors("AllowAll");

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Log.Information("Starting Asset Registry API");
Log.Information("OpenSearch configured: Uri={OpenSearchUri}, IndexFormat={IndexFormat}", openSearchUri, indexFormat);

// Test OpenSearch connection synchronously
try
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true // Accept self-signed certs
    };
    using var httpClient = new HttpClient(handler);
    var authValue = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{openSearchUsername}:{openSearchPassword}"));
    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
    httpClient.Timeout = TimeSpan.FromSeconds(10);

    var testResponse = httpClient.GetAsync(openSearchUri).GetAwaiter().GetResult();
    if (testResponse.IsSuccessStatusCode)
    {
        Log.Information("✓ OpenSearch connection test successful");
    }
    else
    {
        Log.Warning("⚠ OpenSearch connection test returned: {StatusCode} {ReasonPhrase}", testResponse.StatusCode, testResponse.ReasonPhrase);
    }
}
catch (Exception ex)
{
    Log.Error(ex, "✗ OpenSearch connection test failed: {Message}", ex.Message);
    Log.Warning("Logs will still be written to console and file, but OpenSearch logging may not work");
    Console.WriteLine($"[OPENSEARCH CONNECTION TEST] Error: {ex.Message}");
    Console.WriteLine($"[OPENSEARCH CONNECTION TEST] Stack: {ex.StackTrace}");
}

app.Run();
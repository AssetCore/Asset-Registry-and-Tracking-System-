using AuditCompliance.Application.Interfaces;
using AuditCompliance.Application.Services;
using AuditCompliance.Domain.Interfaces;
using AuditCompliance.Infrastructure.Configuration;
using AuditCompliance.Infrastructure.Messaging;
using AuditCompliance.Infrastructure.Persistence;
using AuditCompliance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using Serilog.Sinks.OpenSearch;

var builder = WebApplication.CreateBuilder(args);

// Enable Serilog self-logging to see internal errors
Serilog.Debugging.SelfLog.Enable(Console.Error);

// Configure Serilog with OpenSearch from configuration
var openSearchUri = builder.Configuration["OpenSearch:Uri"] ?? "https://3.150.64.215:9200";
var openSearchUsername = builder.Configuration["OpenSearch:Username"] ?? "admin";
var openSearchPassword = builder.Configuration["OpenSearch:Password"] ?? "MyStrongPassword123!";
var indexFormat = builder.Configuration["OpenSearch:IndexFormat"] ?? "audit-compliance-logs-{0:yyyy.MM.dd}";

var loggerConfig = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "AuditCompliance")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console() // Keep this for debugging
    .WriteTo.File("logs/audit-compliance-log-.txt", rollingInterval: RollingInterval.Day);

// Only add OpenSearch sink if enabled
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
        AutoRegisterTemplate = true,
        MinimumLogEventLevel = Serilog.Events.LogEventLevel.Information
    });
}

Log.Logger = loggerConfig.CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Audit & Compliance API",
        Version = "v1",
        Description = "API for recording and querying audit logs and compliance data"
    });
});

// Database Configuration
builder.Services.AddDbContext<AuditComplianceDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Configure RabbitMQ settings
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection(RabbitMqSettings.SectionName));

// Register repositories
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Register application services
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// Register background service for RabbitMQ consumer
builder.Services.AddHostedService<RabbitMqConsumerService>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AuditComplianceDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger in all environments for easier debugging
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Audit & Compliance API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Prometheus metrics
app.UseHttpMetrics();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");
app.MapMetrics();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuditComplianceDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        Log.Information("Database created/verified successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while creating the database");
    }
}

Log.Information("Starting Audit & Compliance API");
Log.Information("OpenSearch configured: Uri={OpenSearchUri}, IndexFormat={IndexFormat}", openSearchUri, indexFormat);

// Test OpenSearch connection
if (enableOpenSearch)
{
    try
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
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
            Log.Warning("⚠ OpenSearch connection test returned: {StatusCode} {ReasonPhrase}",
                testResponse.StatusCode, testResponse.ReasonPhrase);
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "✗ OpenSearch connection test failed: {Message}", ex.Message);
        Log.Warning("Logs will still be written to console and file, but OpenSearch logging may not work");
    }
}

app.Run();
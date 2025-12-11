using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MaintenanceScheduler.Infrastructure.Data;
using MaintenanceScheduler.Infrastructure;
using MaintenanceScheduler.Domain.Interfaces;
using MaintenanceScheduler.Application.Services;
using MaintenanceScheduler.Application.Validators;
using MaintenanceScheduler.Application.DTOs;
using MaintenanceScheduler.Application.Mappings;
using Serilog;
using Serilog.Sinks.OpenSearch;

var builder = WebApplication.CreateBuilder(args);

// Enable Serilog self-logging to see internal errors
Serilog.Debugging.SelfLog.Enable(Console.Error);



// Configure Serilog with OpenSearch from configuration
var openSearchUri = builder.Configuration["OpenSearch:Uri"] ?? "http://3.150.64.215:9200";
var openSearchUsername = builder.Configuration["OpenSearch:Username"] ?? "admin";
var openSearchPassword = builder.Configuration["OpenSearch:Password"] ?? "MyStrongPassword123!";
var indexFormat = builder.Configuration["OpenSearch:IndexFormat"] ?? "maintenance-logs-{0:yyyy.MM.dd}";


var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console() // Keep this for debugging
    .WriteTo.File("logs/maintenance-log-.txt", rollingInterval: RollingInterval.Day);

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

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Maintenance Scheduler API",
        Version = "v1",
        Description = "API for managing maintenance schedules, history, and warranty information"
    });
});

// Database Configuration
builder.Services.AddDbContext<MaintenanceDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateMaintenanceScheduleValidator>();

// Register Repositories and Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Application Services
builder.Services.AddScoped<IMaintenanceScheduleService, MaintenanceScheduleService>();
builder.Services.AddScoped<IMaintenanceHistoryService, MaintenanceHistoryService>();
builder.Services.AddScoped<IWarrantyInfoService, WarrantyInfoService>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Health Checks
//builder.Services.AddHealthChecks()
//.AddDbContextCheck<MaintenanceDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger in all environments for easier debugging
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Maintenance Scheduler API v1");
    c.RoutePrefix = "swagger"; // Access at /swagger
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

//app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MaintenanceDbContext>();
    try
    {
        // Use EnsureCreated for development - creates database from DbContext model
        dbContext.Database.EnsureCreated();
        Log.Information("Database created/verified successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while creating the database");
    }
}

Log.Information("Starting Maintenance Scheduler API");
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
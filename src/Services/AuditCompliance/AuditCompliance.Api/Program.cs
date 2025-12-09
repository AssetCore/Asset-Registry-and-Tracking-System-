using AuditCompliance.Application.Interfaces;
using AuditCompliance.Application.Services;
using AuditCompliance.Domain.Interfaces;
using AuditCompliance.Infrastructure.Configuration;
using AuditCompliance.Infrastructure.Messaging;
using AuditCompliance.Infrastructure.Persistence;
using AuditCompliance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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
using Serilog;
using Serilog.Sinks.OpenSearch;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with OpenSearch
var openSearchUri = builder.Configuration["OpenSearch:Uri"];
var openSearchUsername = builder.Configuration["OpenSearch:Username"];
var openSearchPassword = builder.Configuration["OpenSearch:Password"];
var indexFormat = builder.Configuration["OpenSearch:IndexFormat"] ?? "audit-compliance-logs-{0:yyyy.MM.dd}";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/audit-compliance-log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri(openSearchUri ?? "http://3.150.64.215:9200"))
    {
        IndexFormat = indexFormat,
        ModifyConnectionSettings = c => c
            .BasicAuthentication(openSearchUsername ?? "admin", openSearchPassword ?? "MyStrongPassword123!")
            .ServerCertificateValidationCallback((o, c, ch, er) => true)
    })
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Audit & Compliance API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuditComplianceDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        Console.WriteLine("Database ensured created.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating database: {ex.Message}");
    }
}

Log.Information("Starting Audit Compliance API");

app.Run();

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

// ---------------------------------------------------------
// ADD THIS NAMESPACE AT THE VERY TOP OF THE FILE:
// using Serilog.Sinks.OpenSearch;
// ---------------------------------------------------------

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Keep this for debugging
    .WriteTo.File("logs/maintenance-log-.txt", rollingInterval: RollingInterval.Day)
// Update the URL to your NEW Public IP
.WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri("http://3.150.64.215:9200"))
{
    IndexFormat = "maintenance-logs-{0:yyyy.MM.dd}",
    ModifyConnectionSettings = c => c
        .BasicAuthentication("admin", "MyStrongPassword123!")
        .ServerCertificateValidationCallback((o, c, ch, er) => true)
})
    .CreateLogger();

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

app.Run();
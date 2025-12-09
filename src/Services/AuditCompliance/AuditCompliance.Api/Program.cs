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

app.Run();

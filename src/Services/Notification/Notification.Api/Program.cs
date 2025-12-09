using Notification.Application.Interfaces;
using Notification.Application.Services;
using Notification.Infrastructure.Configuration;
using Notification.Infrastructure.Services;
using Notification.Infrastructure.Messaging;
using Serilog;
using Serilog.Sinks.OpenSearch;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with OpenSearch
var openSearchUri = builder.Configuration["OpenSearch:Uri"];
var openSearchUsername = builder.Configuration["OpenSearch:Username"];
var openSearchPassword = builder.Configuration["OpenSearch:Password"];
var indexFormat = builder.Configuration["OpenSearch:IndexFormat"] ?? "notification-logs-{0:yyyy.MM.dd}";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/notification-log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri(openSearchUri ?? "http://3.150.64.215:9200"))
    {
        IndexFormat = indexFormat,
        ModifyConnectionSettings = c => c
            .BasicAuthentication(openSearchUsername ?? "admin", openSearchPassword ?? "MyStrongPassword123!")
            .ServerCertificateValidationCallback((o, c, ch, er) => true)
    })
    .CreateLogger();

builder.Host.UseSerilog();

// Add configuration
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection(RabbitMqSettings.SectionName));
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.Configure<SlackSettings>(
    builder.Configuration.GetSection(SlackSettings.SectionName));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application services
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<ISlackService, SlackService>();
builder.Services.AddHttpClient<SlackService>();

// Register background service for RabbitMQ
builder.Services.AddHostedService<RabbitMqConsumerService>();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

Log.Information("Starting Notification API");

app.Run();

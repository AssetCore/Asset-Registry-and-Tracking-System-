using Notification.Application.Interfaces;
using Notification.Application.Services;
using Notification.Infrastructure.Configuration;
using Notification.Infrastructure.Services;
using Notification.Infrastructure.Messaging;
using Prometheus;
using Serilog;
using Serilog.Sinks.OpenSearch;

var builder = WebApplication.CreateBuilder(args);

// Enable Serilog self-logging to see internal errors
Serilog.Debugging.SelfLog.Enable(Console.Error);

// Configure Serilog with OpenSearch from configuration
var openSearchUri = builder.Configuration["OpenSearch:Uri"] ?? "http://3.150.64.215:9200";
var openSearchUsername = builder.Configuration["OpenSearch:Username"] ?? "admin";
var openSearchPassword = builder.Configuration["OpenSearch:Password"] ?? "MyStrongPassword123!";
var indexFormat = builder.Configuration["OpenSearch:IndexFormat"] ?? "notification-logs-{0:yyyy.MM.dd}";

var loggerConfig = new LoggerConfiguration()
     .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "NotificationService")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console() // Keep this for debugging
    .WriteTo.File("logs/notification-log-.txt", rollingInterval: RollingInterval.Day);

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
builder.Services.AddLogging();

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

// Prometheus metrics
app.UseHttpMetrics();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics();

Log.Information("Starting Notification API");
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
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Health;

var builder = WebApplication.CreateBuilder(args);

// Enhanced logging for production debugging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
if (builder.Environment.IsProduction())
{
    builder.Logging.AddJsonConsole(options =>
    {
        options.IncludeScopes = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
    });
}

builder.Services.AddHealthChecks()
    .AddCheck<YarpHealthCheck>("yarp-health");

// Add YARP and load routes/clusters from configuration
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .ConfigureHttpClient((context, handler) =>
    {
        // Configure for long-lived connections to private subnet
        handler.PooledConnectionLifetime = TimeSpan.FromMinutes(10);
        handler.PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5);
        handler.MaxConnectionsPerServer = 100;
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

// Forwarded headers - critical for public subnet deployment
// Preserves original client IP and protocol through the proxy chain
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                                ForwardedHeaders.XForwardedProto | 
                                ForwardedHeaders.XForwardedHost;
    // Trust all proxies in the network path (adjust for security requirements)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = 10; // Increase if more hops in proxy chain
});

// Optional: JWT authentication (configure in appsettings or env vars)
var authSection = builder.Configuration.GetSection("Authentication");
var authority = authSection["Authority"];
var audience = authSection["Audience"];
if (!string.IsNullOrWhiteSpace(authority))
{
    builder.Services
        .AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = authority;
            options.Audience = audience;
            options.RequireHttpsMetadata = true;
        });
    builder.Services.AddAuthorization();
}

// Basic global rate limiting (tune as needed)
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: "global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting LoadBalancer.Api in {Environment} mode", app.Environment.EnvironmentName);

// Enable HTTPS redirection - this IS the external load balancer
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    logger.LogInformation("HTTPS redirection enabled for production");
}

app.UseForwardedHeaders();
app.UseCors();

if (!string.IsNullOrWhiteSpace(authority))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseRateLimiter();

// Enhanced liveness and readiness endpoints
app.MapGet("/", () => Results.Ok(new { 
    Status = "Running",
    Service = "LoadBalancer.Api",
    Version = "1.0.0",
    Timestamp = DateTime.UtcNow
}));

app.MapGet("/health/live", () => Results.Ok(new { Status = "Alive" }))
    .WithName("Liveness")
    .WithMetadata(new Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute(200));

app.MapHealthChecks("/health/ready")
    .WithName("Readiness");

// Legacy health check endpoint
app.MapHealthChecks("/health");

// Reverse proxy endpoint
app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.Use((context, next) =>
    {
        logger.LogDebug("Proxying {Method} {Path} to backend", 
            context.Request.Method, 
            context.Request.Path);
        return next();
    });
});

logger.LogInformation("LoadBalancer.Api started successfully. Listening on {Urls}", 
    string.Join(", ", app.Urls));

app.Run();

public partial class Program { }

// Custom health check for YARP
public class YarpHealthCheck : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
{
    private readonly ILogger<YarpHealthCheck> _logger;

    public YarpHealthCheck(ILogger<YarpHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Add custom health checks here (e.g., check connectivity to Istio gateway)
            return Task.FromResult(
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(
                    "YARP LoadBalancer is healthy"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Task.FromResult(
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(
                    "YARP LoadBalancer is unhealthy", ex));
        }
    }
}

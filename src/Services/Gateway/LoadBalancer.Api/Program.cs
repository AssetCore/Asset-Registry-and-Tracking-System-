using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

// Add YARP and load routes/clusters from configuration
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

// Forwarded headers if running behind another proxy (e.g., Nginx/Front Door)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
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

app.UseHttpsRedirection();
app.UseForwardedHeaders();
app.UseCors();
if (!string.IsNullOrWhiteSpace(authority))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// Prometheus metrics
app.UseHttpMetrics();

app.UseRateLimiter();

// Basic liveness
app.MapGet("/", () => Results.Ok("LoadBalancer.Api is running"));
app.MapHealthChecks("/health");
app.MapMetrics();

// Reverse proxy endpoint
app.MapReverseProxy();

app.Run();

public partial class Program { }

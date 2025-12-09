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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

Log.Information("Starting Audit Compliance API");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

using AsgardeoMicroservice.Services;
using AsgardeoMicroservice.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Sinks.OpenSearch;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with OpenSearch
var openSearchUri = builder.Configuration["OpenSearch:Uri"] ?? "http://3.150.64.215:9200";
var openSearchUsername = builder.Configuration["OpenSearch:Username"] ?? "admin";
var openSearchPassword = builder.Configuration["OpenSearch:Password"] ?? "MyStrongPassword123!";
var indexFormat = builder.Configuration["OpenSearch:IndexFormat"] ?? "identity-access-logs-{0:yyyy.MM.dd}";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/identity-access-log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri(openSearchUri))
    {
        IndexFormat = indexFormat,
        ModifyConnectionSettings = c => c
            .BasicAuthentication(openSearchUsername, openSearchPassword)
            .ServerCertificateValidationCallback((o, c, ch, er) => true)
    })
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// Configure Asgardeo settings
builder.Services.Configure<AsgardeoSettings>(builder.Configuration.GetSection("AsgardeoSettings"));

// Register HTTP client
builder.Services.AddHttpClient<IAsgardeoService, AsgardeoService>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<AsgardeoSettings>>().Value;
    client.BaseAddress = new Uri($"https://api.asgardeo.io/t/{settings.OrganizationName}/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register UserService HTTP client
builder.Services.AddHttpClient<IMeService, MeService>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<AsgardeoSettings>>().Value;
    client.BaseAddress = new Uri($"https://api.asgardeo.io/t/{settings.OrganizationName}/");
    client.DefaultRequestHeaders.Add("Accept", "application/scim+json");
});

// Register services
// Removed redundant AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<ITokenService, TokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

Log.Information("Starting Identity Access API");

app.Run();

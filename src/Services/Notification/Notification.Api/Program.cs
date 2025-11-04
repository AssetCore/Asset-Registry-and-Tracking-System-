using Notification.Application.Interfaces;
using Notification.Application.Services;
using Notification.Infrastructure.Configuration;
using Notification.Infrastructure.Services;
using Notification.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

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
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

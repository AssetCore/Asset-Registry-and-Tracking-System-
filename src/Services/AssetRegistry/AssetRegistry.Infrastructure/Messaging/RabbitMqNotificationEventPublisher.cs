using System.Text;
using System.Text.Json;
using AssetRegistry.Application.Interfaces;
using AssetRegistry.Domain.Events;
using AssetRegistry.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AssetRegistry.Infrastructure.Messaging;

public class RabbitMqNotificationEventPublisher : INotificationEventPublisher, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqNotificationEventPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqNotificationEventPublisher(
        IOptions<RabbitMqSettings> options,
        ILogger<RabbitMqNotificationEventPublisher> logger)
    {
        _settings = options.Value;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Idempotent: if queues already exist, this is fine
        _channel.QueueDeclare(_settings.WarrantyExpiryQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclare(_settings.MaintenanceDueQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclare(_settings.AssetAssignmentQueue, durable: true, exclusive: false, autoDelete: false);
    }

    public Task PublishWarrantyExpiryAsync(AssetWarrantyExpiryEvent evt, CancellationToken ct = default)
        => PublishAsync(_settings.WarrantyExpiryQueue, evt, "warranty.expiry");

    public Task PublishMaintenanceDueAsync(AssetMaintenanceDueEvent evt, CancellationToken ct = default)
        => PublishAsync(_settings.MaintenanceDueQueue, evt, "maintenance.due");

    public Task PublishAssetAssignmentAsync(AssetAssignmentEvent evt, CancellationToken ct = default)
        => PublishAsync(_settings.AssetAssignmentQueue, evt, "asset.assignment");

    private Task PublishAsync<T>(string queueName, T evt, string logEventType)
    {
        var json = JsonSerializer.Serialize(evt);
        var body = Encoding.UTF8.GetBytes(json);

        var props = _channel.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2; // persistent

        // default exchange "" with routingKey = queue name
        _channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: props,
            body: body);

        _logger.LogInformation("Published {EventType} event to queue {QueueName}", logEventType, queueName);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
namespace Notification.Infrastructure.Messaging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Infrastructure.Configuration;
using Notification.Application.Interfaces;
using Notification.Domain.Events;
using System.Text;
using System.Text.Json;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly INotificationService _notificationService;
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqConsumerService(
        IOptions<RabbitMqSettings> rabbitMqSettings,
        INotificationService notificationService,
        ILogger<RabbitMqConsumerService> logger)
    {
        _rabbitMqSettings = rabbitMqSettings.Value;
        _notificationService = notificationService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeRabbitMq();
        
        if (_channel == null)
        {
            _logger.LogError("Failed to initialize RabbitMQ channel");
            return;
        }

        await SetupConsumers(stoppingToken);
    }

    private Task InitializeRabbitMq()
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _rabbitMqSettings.HostName,
                Port = _rabbitMqSettings.Port,
                UserName = _rabbitMqSettings.UserName,
                Password = _rabbitMqSettings.Password,
                VirtualHost = _rabbitMqSettings.VirtualHost
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare queues
            _channel.QueueDeclare(_rabbitMqSettings.WarrantyExpiryQueue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare(_rabbitMqSettings.MaintenanceDueQueue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare(_rabbitMqSettings.AssetAssignmentQueue, durable: true, exclusive: false, autoDelete: false);

            _logger.LogInformation("RabbitMQ connection and channels initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
        }
        
        return Task.CompletedTask;
    }

    private async Task SetupConsumers(CancellationToken stoppingToken)
    {
        if (_channel == null) return;

        // Warranty Expiry Consumer
        var warrantyConsumer = new EventingBasicConsumer(_channel);
        warrantyConsumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var warrantyEvent = JsonSerializer.Deserialize<AssetWarrantyExpiryEvent>(message);

                if (warrantyEvent != null)
                {
                    await _notificationService.ProcessWarrantyExpiryAsync(
                        warrantyEvent.AssetId,
                        warrantyEvent.AssetName,
                        warrantyEvent.OwnerEmail,
                        warrantyEvent.SlackChannel,
                        warrantyEvent.OwnerName,
                        warrantyEvent.WarrantyExpiryDate,
                        warrantyEvent.DaysUntilExpiry);

                    _logger.LogInformation("Processed warranty expiry notification for asset {AssetId}", warrantyEvent.AssetId);
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing warranty expiry message");
                _channel.BasicNack(ea.DeliveryTag, false, true); // Requeue for retry
            }
        };

        _channel.BasicConsume(_rabbitMqSettings.WarrantyExpiryQueue, false, warrantyConsumer);

        // Maintenance Due Consumer
        var maintenanceConsumer = new EventingBasicConsumer(_channel);
        maintenanceConsumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var maintenanceEvent = JsonSerializer.Deserialize<AssetMaintenanceDueEvent>(message);

                if (maintenanceEvent != null)
                {
                    await _notificationService.ProcessMaintenanceDueAsync(
                        maintenanceEvent.AssetId,
                        maintenanceEvent.AssetName,
                        maintenanceEvent.OwnerEmail,
                        maintenanceEvent.SlackChannel,
                        maintenanceEvent.OwnerName,
                        maintenanceEvent.MaintenanceDate,
                        maintenanceEvent.DaysUntilMaintenance);

                    _logger.LogInformation("Processed maintenance due notification for asset {AssetId}", maintenanceEvent.AssetId);
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing maintenance due message");
                _channel.BasicNack(ea.DeliveryTag, false, true); // Requeue for retry
            }
        };

        _channel.BasicConsume(_rabbitMqSettings.MaintenanceDueQueue, false, maintenanceConsumer);

        // Asset Assignment Consumer
        var assignmentConsumer = new EventingBasicConsumer(_channel);
        assignmentConsumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var assignmentEvent = JsonSerializer.Deserialize<AssetAssignmentEvent>(message);

                if (assignmentEvent != null)
                {
                    await _notificationService.ProcessAssetAssignmentAsync(
                        assignmentEvent.AssetId,
                        assignmentEvent.AssetName,
                        assignmentEvent.NewOwnerEmail,
                        assignmentEvent.SlackChannel,
                        assignmentEvent.NewOwnerName,
                        assignmentEvent.AssignmentDate);

                    _logger.LogInformation("Processed asset assignment notification for asset {AssetId}", assignmentEvent.AssetId);
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing asset assignment message");
                _channel.BasicNack(ea.DeliveryTag, false, true); // Requeue for retry
            }
        };

        _channel.BasicConsume(_rabbitMqSettings.AssetAssignmentQueue, false, assignmentConsumer);

        _logger.LogInformation("All RabbitMQ consumers started successfully");

        // Keep the service running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ Consumer Service");
        
        if (_channel != null)
        {
            _channel.Close();
            _channel.Dispose();
        }

        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}
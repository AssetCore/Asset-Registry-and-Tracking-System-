namespace AssetRegistry.Infrastructure.Configuration;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    public string WarrantyExpiryQueue { get; set; } = "warranty_expiry_notifications";
    public string MaintenanceDueQueue { get; set; } = "maintenance_due_notifications";
    public string AssetAssignmentQueue { get; set; } = "asset_assignment_notifications";
}
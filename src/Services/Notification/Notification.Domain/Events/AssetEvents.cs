namespace Notification.Domain.Events;

using Notification.Domain.Entities;

// Event messages that will come from RabbitMQ
public class AssetWarrantyExpiryEvent
{
    public string AssetId { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string SlackChannel { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public DateTime WarrantyExpiryDate { get; set; }
    public int DaysUntilExpiry { get; set; }
}

public class AssetMaintenanceDueEvent
{
    public string AssetId { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string SlackChannel { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public DateTime MaintenanceDate { get; set; }
    public int DaysUntilMaintenance { get; set; }
}

public class AssetAssignmentEvent
{
    public string AssetId { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string NewOwnerEmail { get; set; } = string.Empty;
    public string SlackChannel { get; set; } = string.Empty;
    public string NewOwnerName { get; set; } = string.Empty;
    public DateTime AssignmentDate { get; set; }
}
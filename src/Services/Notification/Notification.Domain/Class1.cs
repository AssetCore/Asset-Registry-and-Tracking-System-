namespace Notification.Domain.Enums;

public enum NotificationType
{
    WarrantyExpiry = 1,
    MaintenanceDue = 2,
    AssetAssignment = 3,
    AssetUpdate = 4
}

public enum NotificationChannel
{
    Email = 1,
    Slack = 2,
    Both = 3
}

public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
    Retry = 4
}

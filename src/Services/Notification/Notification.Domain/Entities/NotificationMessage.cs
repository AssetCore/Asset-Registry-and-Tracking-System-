namespace Notification.Domain.Entities;

using Notification.Domain.Enums;

public class NotificationMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public NotificationType Type { get; set; }
    public NotificationChannel Channel { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    
    // Recipients
    public string EmailAddress { get; set; } = string.Empty;
    public string SlackChannel { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    
    // Content
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    
    // Asset related information
    public string AssetId { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public DateTime? WarrantyExpiryDate { get; set; }
    public DateTime? MaintenanceDate { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }
}
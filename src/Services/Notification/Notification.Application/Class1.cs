namespace Notification.Application.Interfaces;

using Notification.Domain.Entities;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, string? toName = null);
}

public interface ISlackService
{
    Task<bool> SendSlackMessageAsync(string channel, string message, string? userName = null);
}

public interface INotificationService
{
    Task<bool> SendNotificationAsync(NotificationMessage message);
    Task ProcessWarrantyExpiryAsync(string assetId, string assetName, string ownerEmail, 
        string slackChannel, string ownerName, DateTime expiryDate, int daysUntilExpiry);
    Task ProcessMaintenanceDueAsync(string assetId, string assetName, string ownerEmail,
        string slackChannel, string ownerName, DateTime maintenanceDate, int daysUntilMaintenance);
    Task ProcessAssetAssignmentAsync(string assetId, string assetName, string newOwnerEmail,
        string slackChannel, string newOwnerName, DateTime assignmentDate);
}

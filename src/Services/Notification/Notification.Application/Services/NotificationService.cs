namespace Notification.Application.Services;

using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Domain.Events;
using Microsoft.Extensions.Logging;

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEmailService emailService,
        ISmsService smsService,
        ILogger<NotificationService> logger)
    {
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task<bool> SendNotificationAsync(NotificationMessage message)
    {
        try
        {
            var success = true;

            // Send Email if requested
            if (message.Channel == NotificationChannel.Email || message.Channel == NotificationChannel.Both)
            {
                if (!string.IsNullOrEmpty(message.EmailAddress))
                {
                    var emailSent = await _emailService.SendEmailAsync(
                        message.EmailAddress, 
                        message.Subject, 
                        message.Body, 
                        message.RecipientName);
                    
                    if (!emailSent)
                    {
                        success = false;
                        _logger.LogWarning("Failed to send email to {Email} for notification {Id}", 
                            message.EmailAddress, message.Id);
                    }
                }
            }

            // Send SMS if requested
            if (message.Channel == NotificationChannel.SMS || message.Channel == NotificationChannel.Both)
            {
                if (!string.IsNullOrEmpty(message.PhoneNumber))
                {
                    var smsSent = await _smsService.SendSmsAsync(message.PhoneNumber, message.Body);
                    
                    if (!smsSent)
                    {
                        success = false;
                        _logger.LogWarning("Failed to send SMS to {Phone} for notification {Id}", 
                            message.PhoneNumber, message.Id);
                    }
                }
            }

            message.Status = success ? NotificationStatus.Sent : NotificationStatus.Failed;
            message.SentAt = success ? DateTime.UtcNow : null;

            _logger.LogInformation("Notification {Id} processed with status: {Status}", 
                message.Id, message.Status);

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification {Id}", message.Id);
            message.Status = NotificationStatus.Failed;
            message.ErrorMessage = ex.Message;
            return false;
        }
    }

    public async Task ProcessWarrantyExpiryAsync(string assetId, string assetName, string ownerEmail, 
        string ownerPhone, string ownerName, DateTime expiryDate, int daysUntilExpiry)
    {
        var message = new NotificationMessage
        {
            Type = NotificationType.WarrantyExpiry,
            Channel = NotificationChannel.Both, // Send both email and SMS
            EmailAddress = ownerEmail,
            PhoneNumber = ownerPhone,
            RecipientName = ownerName,
            AssetId = assetId,
            AssetName = assetName,
            WarrantyExpiryDate = expiryDate,
            Subject = $"Warranty Expiry Alert: {assetName}",
            Body = GenerateWarrantyExpiryMessage(assetName, expiryDate, daysUntilExpiry)
        };

        await SendNotificationAsync(message);
    }

    public async Task ProcessMaintenanceDueAsync(string assetId, string assetName, string ownerEmail,
        string ownerPhone, string ownerName, DateTime maintenanceDate, int daysUntilMaintenance)
    {
        var message = new NotificationMessage
        {
            Type = NotificationType.MaintenanceDue,
            Channel = NotificationChannel.Both, // Send both email and SMS
            EmailAddress = ownerEmail,
            PhoneNumber = ownerPhone,
            RecipientName = ownerName,
            AssetId = assetId,
            AssetName = assetName,
            MaintenanceDate = maintenanceDate,
            Subject = $"Maintenance Due Alert: {assetName}",
            Body = GenerateMaintenanceDueMessage(assetName, maintenanceDate, daysUntilMaintenance)
        };

        await SendNotificationAsync(message);
    }

    public async Task ProcessAssetAssignmentAsync(string assetId, string assetName, string newOwnerEmail,
        string newOwnerPhone, string newOwnerName, DateTime assignmentDate)
    {
        var message = new NotificationMessage
        {
            Type = NotificationType.AssetAssignment,
            Channel = NotificationChannel.Email, // Only email for assignment
            EmailAddress = newOwnerEmail,
            PhoneNumber = newOwnerPhone,
            RecipientName = newOwnerName,
            AssetId = assetId,
            AssetName = assetName,
            Subject = $"Asset Assignment: {assetName}",
            Body = GenerateAssetAssignmentMessage(assetName, newOwnerName, assignmentDate)
        };

        await SendNotificationAsync(message);
    }

    private static string GenerateWarrantyExpiryMessage(string assetName, DateTime expiryDate, int daysUntilExpiry)
    {
        if (daysUntilExpiry <= 0)
        {
            return $"URGENT: The warranty for your asset '{assetName}' has expired on {expiryDate:yyyy-MM-dd}. " +
                   "Please contact your IT department immediately to arrange for warranty renewal or service options.";
        }
        else if (daysUntilExpiry <= 7)
        {
            return $"URGENT: The warranty for your asset '{assetName}' will expire in {daysUntilExpiry} day(s) on {expiryDate:yyyy-MM-dd}. " +
                   "Please contact your IT department to arrange for warranty renewal.";
        }
        else
        {
            return $"REMINDER: The warranty for your asset '{assetName}' will expire in {daysUntilExpiry} days on {expiryDate:yyyy-MM-dd}. " +
                   "Please plan for warranty renewal to ensure continued support.";
        }
    }

    private static string GenerateMaintenanceDueMessage(string assetName, DateTime maintenanceDate, int daysUntilMaintenance)
    {
        if (daysUntilMaintenance <= 0)
        {
            return $"URGENT: Maintenance for your asset '{assetName}' is overdue (was due on {maintenanceDate:yyyy-MM-dd}). " +
                   "Please schedule maintenance immediately to prevent potential issues.";
        }
        else if (daysUntilMaintenance <= 3)
        {
            return $"URGENT: Maintenance for your asset '{assetName}' is due in {daysUntilMaintenance} day(s) on {maintenanceDate:yyyy-MM-dd}. " +
                   "Please schedule the maintenance appointment as soon as possible.";
        }
        else
        {
            return $"REMINDER: Maintenance for your asset '{assetName}' is due in {daysUntilMaintenance} days on {maintenanceDate:yyyy-MM-dd}. " +
                   "Please schedule your maintenance appointment to ensure optimal performance.";
        }
    }

    private static string GenerateAssetAssignmentMessage(string assetName, string ownerName, DateTime assignmentDate)
    {
        return $"Hello {ownerName},\n\n" +
               $"You have been assigned a new asset: '{assetName}' as of {assignmentDate:yyyy-MM-dd}.\n\n" +
               "Please ensure you:\n" +
               "- Take proper care of the asset\n" +
               "- Report any issues immediately\n" +
               "- Follow company asset management policies\n\n" +
               "If you have any questions, please contact your IT department.\n\n" +
               "Thank you!";
    }
}
namespace Notification.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Enums;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationService notificationService,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<ActionResult> SendNotification([FromBody] SendNotificationRequest request)
    {
        try
        {
            var message = new NotificationMessage
            {
                Type = request.Type,
                Channel = request.Channel,
                EmailAddress = request.EmailAddress ?? string.Empty,
                PhoneNumber = request.PhoneNumber ?? string.Empty,
                RecipientName = request.RecipientName ?? string.Empty,
                Subject = request.Subject,
                Body = request.Body,
                AssetId = request.AssetId ?? string.Empty,
                AssetName = request.AssetName ?? string.Empty
            };

            var success = await _notificationService.SendNotificationAsync(message);

            if (success)
            {
                return Ok(new { Message = "Notification sent successfully", NotificationId = message.Id });
            }

            return BadRequest(new { Message = "Failed to send notification" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpPost("warranty-expiry")]
    public async Task<ActionResult> SendWarrantyExpiryNotification([FromBody] WarrantyExpiryNotificationRequest request)
    {
        try
        {
            await _notificationService.ProcessWarrantyExpiryAsync(
                request.AssetId,
                request.AssetName,
                request.OwnerEmail,
                request.OwnerPhone ?? string.Empty,
                request.OwnerName,
                request.WarrantyExpiryDate,
                request.DaysUntilExpiry);

            return Ok(new { Message = "Warranty expiry notification processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing warranty expiry notification");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpPost("maintenance-due")]
    public async Task<ActionResult> SendMaintenanceDueNotification([FromBody] MaintenanceDueNotificationRequest request)
    {
        try
        {
            await _notificationService.ProcessMaintenanceDueAsync(
                request.AssetId,
                request.AssetName,
                request.OwnerEmail,
                request.OwnerPhone ?? string.Empty,
                request.OwnerName,
                request.MaintenanceDate,
                request.DaysUntilMaintenance);

            return Ok(new { Message = "Maintenance due notification processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing maintenance due notification");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpPost("asset-assignment")]
    public async Task<ActionResult> SendAssetAssignmentNotification([FromBody] AssetAssignmentNotificationRequest request)
    {
        try
        {
            await _notificationService.ProcessAssetAssignmentAsync(
                request.AssetId,
                request.AssetName,
                request.NewOwnerEmail,
                request.NewOwnerPhone ?? string.Empty,
                request.NewOwnerName,
                request.AssignmentDate);

            return Ok(new { Message = "Asset assignment notification processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing asset assignment notification");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpGet("health")]
    public ActionResult GetHealth()
    {
        return Ok(new { 
            Status = "Healthy", 
            Service = "Notification Service",
            Timestamp = DateTime.UtcNow 
        });
    }
}

// Request DTOs
public record SendNotificationRequest(
    NotificationType Type,
    NotificationChannel Channel,
    string? EmailAddress,
    string? PhoneNumber,
    string? RecipientName,
    string Subject,
    string Body,
    string? AssetId,
    string? AssetName
);

public record WarrantyExpiryNotificationRequest(
    string AssetId,
    string AssetName,
    string OwnerEmail,
    string? OwnerPhone,
    string OwnerName,
    DateTime WarrantyExpiryDate,
    int DaysUntilExpiry
);

public record MaintenanceDueNotificationRequest(
    string AssetId,
    string AssetName,
    string OwnerEmail,
    string? OwnerPhone,
    string OwnerName,
    DateTime MaintenanceDate,
    int DaysUntilMaintenance
);

public record AssetAssignmentNotificationRequest(
    string AssetId,
    string AssetName,
    string NewOwnerEmail,
    string? NewOwnerPhone,
    string NewOwnerName,
    DateTime AssignmentDate
);
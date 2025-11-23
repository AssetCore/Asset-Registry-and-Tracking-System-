using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MaintenanceScheduler.Application.Services;

namespace MaintenanceScheduler.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<NotificationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendMaintenanceReminderAsync(
            Guid scheduleId, string assetName, DateTime scheduledDate, string assignedTo)
        {
            try
            {
                var notification = new
                {
                    Type = "MaintenanceReminder",
                    ScheduleId = scheduleId,
                    Title = $"Maintenance Reminder: {assetName}",
                    Message = $"Scheduled maintenance for {assetName} is due on {scheduledDate:yyyy-MM-dd HH:mm}",
                    Recipient = assignedTo,
                    Priority = "Normal",
                    ScheduledDate = scheduledDate
                };

                await SendNotificationAsync(notification);
                _logger.LogInformation(
                    "Maintenance reminder sent for schedule {ScheduleId}, Asset: {AssetName}",
                    scheduleId, assetName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send maintenance reminder for schedule {ScheduleId}", scheduleId);
            }
        }

        public async Task SendWarrantyExpiryReminderAsync(
            Guid warrantyId, string assetName, DateTime expiryDate, string contactEmail)
        {
            try
            {
                var daysUntilExpiry = (expiryDate - DateTime.UtcNow).Days;
                var notification = new
                {
                    Type = "WarrantyExpiryReminder",
                    WarrantyId = warrantyId,
                    Title = $"Warranty Expiring Soon: {assetName}",
                    Message = $"Warranty for {assetName} will expire in {daysUntilExpiry} days on {expiryDate:yyyy-MM-dd}",
                    Recipient = contactEmail,
                    Priority = daysUntilExpiry <= 7 ? "High" : "Normal",
                    ExpiryDate = expiryDate
                };

                await SendNotificationAsync(notification);
                _logger.LogInformation(
                    "Warranty expiry reminder sent for warranty {WarrantyId}, Asset: {AssetName}",
                    warrantyId, assetName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send warranty expiry reminder for warranty {WarrantyId}", warrantyId);
            }
        }

        public async Task SendOverdueMaintenanceAlertAsync(
            Guid scheduleId, string assetName, DateTime scheduledDate, string assignedTo)
        {
            try
            {
                var daysOverdue = (DateTime.UtcNow - scheduledDate).Days;
                var notification = new
                {
                    Type = "OverdueMaintenanceAlert",
                    ScheduleId = scheduleId,
                    Title = $"URGENT: Overdue Maintenance - {assetName}",
                    Message = $"Maintenance for {assetName} is {daysOverdue} days overdue. Originally scheduled for {scheduledDate:yyyy-MM-dd}",
                    Recipient = assignedTo,
                    Priority = "High",
                    DaysOverdue = daysOverdue
                };

                await SendNotificationAsync(notification);
                _logger.LogWarning(
                    "Overdue maintenance alert sent for schedule {ScheduleId}, Asset: {AssetName}, Days overdue: {DaysOverdue}",
                    scheduleId, assetName, daysOverdue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send overdue maintenance alert for schedule {ScheduleId}", scheduleId);
            }
        }

        private async Task SendNotificationAsync(object notification)
        {
            var client = _httpClientFactory.CreateClient("NotificationService");
            var notificationServiceUrl = _configuration["NotificationService:BaseUrl"];

            if (string.IsNullOrEmpty(notificationServiceUrl))
            {
                _logger.LogWarning("Notification service URL not configured. Skipping notification.");
                return;
            }

            var json = JsonSerializer.Serialize(notification);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/notifications", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to send notification. Status code: {StatusCode}, Response: {Response}",
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync());
            }
        }
    }
}
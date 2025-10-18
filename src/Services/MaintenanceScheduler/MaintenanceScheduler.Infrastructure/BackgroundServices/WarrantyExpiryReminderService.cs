using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MaintenanceScheduler.Domain.Interfaces;
using MaintenanceScheduler.Application.Services;

namespace MaintenanceScheduler.Infrastructure.BackgroundServices
{
    public class WarrantyExpiryReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WarrantyExpiryReminderService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1); // Check daily
        private readonly int _reminderDaysAhead = 30; // Send reminders 30 days ahead

        public WarrantyExpiryReminderService(
            IServiceProvider serviceProvider,
            ILogger<WarrantyExpiryReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Warranty Expiry Reminder Service is starting.");

            // Wait for 1 minute before starting to allow the application to fully start
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessWarrantyExpiryRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing warranty expiry reminders");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Warranty Expiry Reminder Service is stopping.");
        }

        private async Task ProcessWarrantyExpiryRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var expiringWarranties = await unitOfWork.WarrantyInfos
                .GetExpiringWarrantiesAsync(_reminderDaysAhead);

            foreach (var warranty in expiringWarranties)
            {
                try
                {
                    // Check if notification was already sent recently (within last 7 days)
                    if (warranty.NotificationSent &&
                        warranty.LastNotificationDate.HasValue &&
                        (DateTime.UtcNow - warranty.LastNotificationDate.Value).Days < 7)
                    {
                        continue;
                    }

                    await notificationService.SendWarrantyExpiryReminderAsync(
                        warranty.Id,
                        warranty.AssetName,
                        warranty.ExpiryDate,
                        warranty.ContactEmail ?? "warranty-team@company.com");

                    // Update notification status
                    warranty.NotificationSent = true;
                    warranty.LastNotificationDate = DateTime.UtcNow;
                    await unitOfWork.WarrantyInfos.UpdateAsync(warranty);
                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send warranty expiry reminder for warranty {WarrantyId}",
                        warranty.Id);
                }
            }

            if (expiringWarranties.Any())
            {
                _logger.LogInformation(
                    "Processed {Count} warranty expiry reminders",
                    expiringWarranties.Count());
            }
        }
    }
}
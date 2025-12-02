using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MaintenanceScheduler.Domain.Interfaces;
using MaintenanceScheduler.Application.Services;

namespace MaintenanceScheduler.Infrastructure.BackgroundServices
{
    public class MaintenanceReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MaintenanceReminderService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Check every 6 hours
        private readonly int _reminderDaysAhead = 3; // Send reminders 3 days ahead

        public MaintenanceReminderService(
            IServiceProvider serviceProvider,
            ILogger<MaintenanceReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Maintenance Reminder Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessMaintenanceRemindersAsync();
                    await ProcessOverdueMaintenanceAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing maintenance reminders");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Maintenance Reminder Service is stopping.");
        }

        private async Task ProcessMaintenanceRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var upcomingMaintenance = await unitOfWork.MaintenanceSchedules
                .GetUpcomingAsync(_reminderDaysAhead);

            foreach (var schedule in upcomingMaintenance)
            {
                try
                {
                    await notificationService.SendMaintenanceReminderAsync(
                        schedule.Id,
                        schedule.AssetName,
                        schedule.ScheduledDate,
                        schedule.AssignedTo ?? "Maintenance Team");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send reminder for maintenance schedule {ScheduleId}",
                        schedule.Id);
                }
            }

            if (upcomingMaintenance.Any())
            {
                _logger.LogInformation(
                    "Processed {Count} upcoming maintenance reminders",
                    upcomingMaintenance.Count());
            }
        }

        private async Task ProcessOverdueMaintenanceAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var overdueMaintenance = await unitOfWork.MaintenanceSchedules.GetOverdueAsync();

            foreach (var schedule in overdueMaintenance)
            {
                try
                {
                    await notificationService.SendOverdueMaintenanceAlertAsync(
                        schedule.Id,
                        schedule.AssetName,
                        schedule.ScheduledDate,
                        schedule.AssignedTo ?? "Maintenance Team");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send overdue alert for maintenance schedule {ScheduleId}",
                        schedule.Id);
                }
            }

            if (overdueMaintenance.Any())
            {
                _logger.LogWarning(
                    "Processed {Count} overdue maintenance alerts",
                    overdueMaintenance.Count());
            }
        }
    }
}
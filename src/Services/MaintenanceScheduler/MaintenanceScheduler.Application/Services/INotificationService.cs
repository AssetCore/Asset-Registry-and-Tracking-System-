using System;
using System.Threading.Tasks;

namespace MaintenanceScheduler.Application.Services
{
    public interface INotificationService
    {
        Task SendMaintenanceReminderAsync(Guid scheduleId, string assetName, DateTime scheduledDate, string assignedTo);
        Task SendWarrantyExpiryReminderAsync(Guid warrantyId, string assetName, DateTime expiryDate, string contactEmail);
        Task SendOverdueMaintenanceAlertAsync(Guid scheduleId, string assetName, DateTime scheduledDate, string assignedTo);
    }
}
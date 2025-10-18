using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaintenanceScheduler.Domain.Entities;

namespace MaintenanceScheduler.Domain.Interfaces
{
    public interface IMaintenanceScheduleRepository
    {
        Task<MaintenanceSchedule> GetByIdAsync(Guid id);
        Task<IEnumerable<MaintenanceSchedule>> GetAllAsync();
        Task<IEnumerable<MaintenanceSchedule>> GetByAssetIdAsync(Guid assetId);
        Task<IEnumerable<MaintenanceSchedule>> GetUpcomingAsync(int days);
        Task<IEnumerable<MaintenanceSchedule>> GetOverdueAsync();
        Task<IEnumerable<MaintenanceSchedule>> GetByStatusAsync(MaintenanceStatus status);
        Task<MaintenanceSchedule> AddAsync(MaintenanceSchedule schedule);
        Task<MaintenanceSchedule> UpdateAsync(MaintenanceSchedule schedule);
        Task<bool> DeleteAsync(Guid id);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaintenanceScheduler.Domain.Entities;

namespace MaintenanceScheduler.Domain.Interfaces
{
    public interface IMaintenanceHistoryRepository
    {
        Task<MaintenanceHistory> GetByIdAsync(Guid id);
        Task<IEnumerable<MaintenanceHistory>> GetByAssetIdAsync(Guid assetId);
        Task<IEnumerable<MaintenanceHistory>> GetByDateRangeAsync(Guid assetId, DateTime startDate, DateTime endDate);
        Task<MaintenanceHistory> AddAsync(MaintenanceHistory history);
        Task<MaintenanceHistory> UpdateAsync(MaintenanceHistory history);
        Task<bool> DeleteAsync(Guid id);
    }
}
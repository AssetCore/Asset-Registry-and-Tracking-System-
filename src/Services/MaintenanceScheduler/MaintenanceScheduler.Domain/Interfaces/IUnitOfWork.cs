using System;
using System.Threading.Tasks;

namespace MaintenanceScheduler.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IMaintenanceScheduleRepository MaintenanceSchedules { get; }
        IMaintenanceHistoryRepository MaintenanceHistories { get; }
        IWarrantyInfoRepository WarrantyInfos { get; }
        Task<int> SaveChangesAsync();
    }
}
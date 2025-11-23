using System;
using System.Threading.Tasks;
using MaintenanceScheduler.Domain.Interfaces;
using MaintenanceScheduler.Infrastructure.Data;
using MaintenanceScheduler.Infrastructure.Repositories;

namespace MaintenanceScheduler.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MaintenanceDbContext _context;
        private IMaintenanceScheduleRepository? _maintenanceSchedules;
        private IMaintenanceHistoryRepository? _maintenanceHistories;
        private IWarrantyInfoRepository? _warrantyInfos;

        public UnitOfWork(MaintenanceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IMaintenanceScheduleRepository MaintenanceSchedules
        {
            get
            {
                return _maintenanceSchedules ??= new MaintenanceScheduleRepository(_context);
            }
        }

        public IMaintenanceHistoryRepository MaintenanceHistories
        {
            get
            {
                return _maintenanceHistories ??= new MaintenanceHistoryRepository(_context);
            }
        }

        public IWarrantyInfoRepository WarrantyInfos
        {
            get
            {
                return _warrantyInfos ??= new WarrantyInfoRepository(_context);
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
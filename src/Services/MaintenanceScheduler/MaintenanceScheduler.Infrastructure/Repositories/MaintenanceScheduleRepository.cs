using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MaintenanceScheduler.Domain.Entities;
using MaintenanceScheduler.Domain.Interfaces;
using MaintenanceScheduler.Infrastructure.Data;

namespace MaintenanceScheduler.Infrastructure.Repositories
{
    public class MaintenanceScheduleRepository : IMaintenanceScheduleRepository
    {
        private readonly MaintenanceDbContext _context;

        public MaintenanceScheduleRepository(MaintenanceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<MaintenanceSchedule> GetByIdAsync(Guid id)
        {
            var schedule = await _context.MaintenanceSchedules
                .Include(m => m.MaintenanceHistories)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (schedule == null)
                throw new InvalidOperationException($"MaintenanceSchedule with Id '{id}' not found.");

            return schedule;
        }

        public async Task<IEnumerable<MaintenanceSchedule>> GetAllAsync()
        {
            return await _context.MaintenanceSchedules
                .Include(m => m.MaintenanceHistories)
                .OrderBy(m => m.ScheduledDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaintenanceSchedule>> GetByAssetIdAsync(Guid assetId)
        {
            return await _context.MaintenanceSchedules
                .Include(m => m.MaintenanceHistories)
                .Where(m => m.AssetId == assetId)
                .OrderByDescending(m => m.ScheduledDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaintenanceSchedule>> GetUpcomingAsync(int days)
        {
            var endDate = DateTime.UtcNow.AddDays(days);
            return await _context.MaintenanceSchedules
                .Where(m => m.ScheduledDate >= DateTime.UtcNow &&
                           m.ScheduledDate <= endDate &&
                           m.Status == MaintenanceStatus.Scheduled)
                .OrderBy(m => m.ScheduledDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaintenanceSchedule>> GetOverdueAsync()
        {
            return await _context.MaintenanceSchedules
                .Where(m => m.ScheduledDate < DateTime.UtcNow &&
                           m.Status == MaintenanceStatus.Scheduled)
                .OrderBy(m => m.ScheduledDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaintenanceSchedule>> GetByStatusAsync(MaintenanceStatus status)
        {
            return await _context.MaintenanceSchedules
                .Where(m => m.Status == status)
                .OrderBy(m => m.ScheduledDate)
                .ToListAsync();
        }

        public async Task<MaintenanceSchedule> AddAsync(MaintenanceSchedule schedule)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));

            schedule.Id = Guid.NewGuid();
            schedule.CreatedAt = DateTime.UtcNow;

            await _context.MaintenanceSchedules.AddAsync(schedule);
            return schedule;
        }

        public async Task<MaintenanceSchedule> UpdateAsync(MaintenanceSchedule schedule)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));

            schedule.UpdatedAt = DateTime.UtcNow;
            _context.MaintenanceSchedules.Update(schedule);
            return await Task.FromResult(schedule);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var schedule = await _context.MaintenanceSchedules.FindAsync(id);
            if (schedule == null)
                return false;

            _context.MaintenanceSchedules.Remove(schedule);
            return true;
        }
    }
}
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
    public class MaintenanceHistoryRepository : IMaintenanceHistoryRepository
    {
        private  readonly MaintenanceDbContext _context;

        public MaintenanceHistoryRepository(MaintenanceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<MaintenanceHistory> GetByIdAsync(Guid id)
        {
            var history = await _context.MaintenanceHistories
                .Include(h => h.MaintenanceSchedule)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (history == null)
                throw new InvalidOperationException($"MaintenanceHistory with Id '{id}' not found.");

            return history;
        }

        public async Task<IEnumerable<MaintenanceHistory>> GetByAssetIdAsync(Guid assetId)
        {
            return await _context.MaintenanceHistories
                .Where(h => h.AssetId == assetId)
                .OrderByDescending(h => h.MaintenanceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaintenanceHistory>> GetByDateRangeAsync(
            Guid assetId, DateTime startDate, DateTime endDate)
        {
            return await _context.MaintenanceHistories
                .Where(h => h.AssetId == assetId &&
                           h.MaintenanceDate >= startDate &&
                           h.MaintenanceDate <= endDate)
                .OrderByDescending(h => h.MaintenanceDate)
                .ToListAsync();
        }

        public async Task<MaintenanceHistory> AddAsync(MaintenanceHistory history)
        {
            if (history == null)
                throw new ArgumentNullException(nameof(history));

            history.Id = Guid.NewGuid();
            history.CreatedAt = DateTime.UtcNow;

            await _context.MaintenanceHistories.AddAsync(history);
            return history;
        }

        public async Task<MaintenanceHistory> UpdateAsync(MaintenanceHistory history)
        {
            if (history == null)
                throw new ArgumentNullException(nameof(history));

            _context.MaintenanceHistories.Update(history);
            return await Task.FromResult(history);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var history = await _context.MaintenanceHistories.FindAsync(id);
            if (history == null)
                return false;

            _context.MaintenanceHistories.Remove(history);
            return true;
        }
    }
}
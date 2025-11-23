using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaintenanceScheduler.Application.DTOs;

namespace MaintenanceScheduler.Application.Services
{
    public interface IMaintenanceHistoryService
    {
        Task<ApiResponse<MaintenanceHistoryDto>> GetByIdAsync(Guid id);
        Task<ApiResponse<IEnumerable<MaintenanceHistoryDto>>> GetByAssetIdAsync(Guid assetId);
        Task<ApiResponse<IEnumerable<MaintenanceHistoryDto>>> GetByDateRangeAsync(
            Guid assetId, DateTime startDate, DateTime endDate);
        Task<ApiResponse<MaintenanceHistoryDto>> CreateAsync(CreateMaintenanceHistoryDto dto, string userId);
        Task<ApiResponse<bool>> DeleteAsync(Guid id);
    }
}

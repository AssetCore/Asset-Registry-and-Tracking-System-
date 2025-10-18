using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaintenanceScheduler.Application.DTOs;

namespace MaintenanceScheduler.Application.Services
{
    public interface IMaintenanceScheduleService
    {
        Task<ApiResponse<MaintenanceScheduleDto>> GetByIdAsync(Guid id);
        Task<ApiResponse<IEnumerable<MaintenanceScheduleDto>>> GetAllAsync();
        Task<ApiResponse<IEnumerable<MaintenanceScheduleDto>>> GetByAssetIdAsync(Guid assetId);
        Task<ApiResponse<IEnumerable<MaintenanceScheduleDto>>> GetUpcomingAsync(int days);
        Task<ApiResponse<IEnumerable<MaintenanceScheduleDto>>> GetOverdueAsync();
        Task<ApiResponse<MaintenanceScheduleDto>> CreateAsync(CreateMaintenanceScheduleDto dto, string userId);
        Task<ApiResponse<MaintenanceScheduleDto>> UpdateAsync(UpdateMaintenanceScheduleDto dto, string userId);
        Task<ApiResponse<bool>> DeleteAsync(Guid id);
        Task<ApiResponse<MaintenanceScheduleDto>> CompleteMaintenanceAsync(Guid id, decimal actualCost, string userId);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaintenanceScheduler.Application.DTOs;

namespace MaintenanceScheduler.Application.Services
{
    public interface IWarrantyInfoService
    {
        Task<ApiResponse<WarrantyInfoDto>> GetByIdAsync(Guid id);
        Task<ApiResponse<WarrantyInfoDto>> GetByAssetIdAsync(Guid assetId);
        Task<ApiResponse<IEnumerable<WarrantyInfoDto>>> GetExpiringWarrantiesAsync(int days);
        Task<ApiResponse<IEnumerable<WarrantyInfoDto>>> GetActiveWarrantiesAsync();
        Task<ApiResponse<WarrantyInfoDto>> CreateAsync(CreateWarrantyInfoDto dto, string userId);
        Task<ApiResponse<WarrantyInfoDto>> UpdateAsync(UpdateWarrantyInfoDto dto, string userId);
        Task<ApiResponse<bool>> DeleteAsync(Guid id);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceScheduler.Application.DTOs;
using MaintenanceScheduler.Domain.Entities;
using MaintenanceScheduler.Domain.Interfaces;

namespace MaintenanceScheduler.Application.Services
{
    public class MaintenanceScheduleService : IMaintenanceScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MaintenanceScheduleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponse<MaintenanceScheduleDto>> GetByIdAsync(Guid id)
        {
            try
            {
                var schedule = await _unitOfWork.MaintenanceSchedules.GetByIdAsync(id);
                if (schedule == null)
                {
                    return ApiResponse<MaintenanceScheduleDto>.ErrorResponse("Maintenance schedule not found");
                }

                var dto = _mapper.Map<MaintenanceScheduleDto>(schedule);
                return ApiResponse<MaintenanceScheduleDto>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<MaintenanceScheduleDto>.ErrorResponse(
                    "An error occurred while retrieving the maintenance schedule",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<IEnumerable<MaintenanceScheduleDto>>> GetAllAsync()
        {
            try
            {
                var schedules = await _unitOfWork.MaintenanceSchedules.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<MaintenanceScheduleDto>>(schedules);
                return ApiResponse<IEnumerable<MaintenanceScheduleDto>>.SuccessResponse(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<MaintenanceScheduleDto>>.ErrorResponse(
                    "An error occurred while retrieving maintenance schedules",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<IEnumerable<MaintenanceScheduleDto>>> GetByAssetIdAsync(Guid assetId)
        {
            try
            {
                var schedules = await _unitOfWork.MaintenanceSchedules.GetByAssetIdAsync(assetId);
                var dtos = _mapper.Map<IEnumerable<MaintenanceScheduleDto>>(schedules);
                return ApiResponse<IEnumerable<MaintenanceScheduleDto>>.SuccessResponse(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<MaintenanceScheduleDto>>.ErrorResponse(
                    "An error occurred while retrieving maintenance schedules",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<IEnumerable<MaintenanceScheduleDto>>> GetUpcomingAsync(int days)
        {
            try
            {
                var schedules = await _unitOfWork.MaintenanceSchedules.GetUpcomingAsync(days);
                var dtos = _mapper.Map<IEnumerable<MaintenanceScheduleDto>>(schedules);
                return ApiResponse<IEnumerable<MaintenanceScheduleDto>>.SuccessResponse(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<MaintenanceScheduleDto>>.ErrorResponse(
                    "An error occurred while retrieving upcoming maintenance schedules",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<IEnumerable<MaintenanceScheduleDto>>> GetOverdueAsync()
        {
            try
            {
                var schedules = await _unitOfWork.MaintenanceSchedules.GetOverdueAsync();
                var dtos = _mapper.Map<IEnumerable<MaintenanceScheduleDto>>(schedules);
                return ApiResponse<IEnumerable<MaintenanceScheduleDto>>.SuccessResponse(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<MaintenanceScheduleDto>>.ErrorResponse(
                    "An error occurred while retrieving overdue maintenance schedules",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<MaintenanceScheduleDto>> CreateAsync(
            CreateMaintenanceScheduleDto dto, string userId)
        {
            try
            {
                var schedule = _mapper.Map<MaintenanceSchedule>(dto);
                schedule.Status = MaintenanceStatus.Scheduled;
                schedule.CreatedBy = userId;

                if (schedule.FrequencyInDays > 0)
                {
                    schedule.NextScheduledDate = schedule.ScheduledDate.AddDays(schedule.FrequencyInDays);
                }

                await _unitOfWork.MaintenanceSchedules.AddAsync(schedule);
                await _unitOfWork.SaveChangesAsync();

                var resultDto = _mapper.Map<MaintenanceScheduleDto>(schedule);
                return ApiResponse<MaintenanceScheduleDto>.SuccessResponse(
                    resultDto, "Maintenance schedule created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<MaintenanceScheduleDto>.ErrorResponse(
                    "An error occurred while creating the maintenance schedule",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<MaintenanceScheduleDto>> UpdateAsync(
            UpdateMaintenanceScheduleDto dto, string userId)
        {
            try
            {
                var schedule = await _unitOfWork.MaintenanceSchedules.GetByIdAsync(dto.Id);
                if (schedule == null)
                {
                    return ApiResponse<MaintenanceScheduleDto>.ErrorResponse("Maintenance schedule not found");
                }

                schedule.Description = dto.Description;
                schedule.ScheduledDate = dto.ScheduledDate;
                schedule.Status = dto.Status;
                schedule.AssignedTo = dto.AssignedTo;
                schedule.EstimatedCost = dto.EstimatedCost;
                schedule.ActualCost = dto.ActualCost;
                schedule.Notes = dto.Notes;
                schedule.UpdatedBy = userId;

                await _unitOfWork.MaintenanceSchedules.UpdateAsync(schedule);
                await _unitOfWork.SaveChangesAsync();

                var resultDto = _mapper.Map<MaintenanceScheduleDto>(schedule);
                return ApiResponse<MaintenanceScheduleDto>.SuccessResponse(
                    resultDto, "Maintenance schedule updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<MaintenanceScheduleDto>.ErrorResponse(
                    "An error occurred while updating the maintenance schedule",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var result = await _unitOfWork.MaintenanceSchedules.DeleteAsync(id);
                if (!result)
                {
                    return ApiResponse<bool>.ErrorResponse("Maintenance schedule not found");
                }

                await _unitOfWork.SaveChangesAsync();
                return ApiResponse<bool>.SuccessResponse(true, "Maintenance schedule deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse(
                    "An error occurred while deleting the maintenance schedule",
                    new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<MaintenanceScheduleDto>> CompleteMaintenanceAsync(
            Guid id, decimal actualCost, string userId)
        {
            try
            {
                var schedule = await _unitOfWork.MaintenanceSchedules.GetByIdAsync(id);
                if (schedule == null)
                {
                    return ApiResponse<MaintenanceScheduleDto>.ErrorResponse("Maintenance schedule not found");
                }

                schedule.Status = MaintenanceStatus.Completed;
                schedule.CompletedDate = DateTime.UtcNow;
                schedule.ActualCost = actualCost;
                schedule.UpdatedBy = userId;

                if (schedule.FrequencyInDays > 0 && schedule.NextScheduledDate.HasValue)
                {
                    var newSchedule = new MaintenanceSchedule
                    {
                        AssetId = schedule.AssetId,
                        AssetName = schedule.AssetName,
                        MaintenanceType = schedule.MaintenanceType,
                        Description = schedule.Description,
                        ScheduledDate = schedule.NextScheduledDate.Value,
                        Status = MaintenanceStatus.Scheduled,
                        FrequencyInDays = schedule.FrequencyInDays,
                        NextScheduledDate = schedule.NextScheduledDate.Value.AddDays(schedule.FrequencyInDays),
                        AssignedTo = schedule.AssignedTo,
                        EstimatedCost = schedule.EstimatedCost,
                        CreatedBy = userId,
                        UpdatedBy = userId, // Required member
                        Notes = schedule.Notes, // Required member
                        MaintenanceHistories = new List<MaintenanceHistory>() // Required member
                    };

                    await _unitOfWork.MaintenanceSchedules.AddAsync(newSchedule);
                }

                await _unitOfWork.MaintenanceSchedules.UpdateAsync(schedule);
                await _unitOfWork.SaveChangesAsync();

                var resultDto = _mapper.Map<MaintenanceScheduleDto>(schedule);
                return ApiResponse<MaintenanceScheduleDto>.SuccessResponse(
                    resultDto, "Maintenance completed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<MaintenanceScheduleDto>.ErrorResponse(
                    "An error occurred while completing the maintenance",
                    new List<string> { ex.Message });
            }
        }
    }
}
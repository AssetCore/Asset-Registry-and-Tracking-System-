using AutoMapper;
using MaintenanceScheduler.Application.DTOs;
using MaintenanceScheduler.Domain.Entities;

namespace MaintenanceScheduler.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // MaintenanceSchedule mappings
            CreateMap<MaintenanceSchedule, MaintenanceScheduleDto>();
            CreateMap<CreateMaintenanceScheduleDto, MaintenanceSchedule>();
            CreateMap<UpdateMaintenanceScheduleDto, MaintenanceSchedule>();

            // MaintenanceHistory mappings
            CreateMap<MaintenanceHistory, MaintenanceHistoryDto>();
            CreateMap<CreateMaintenanceHistoryDto, MaintenanceHistory>();

            // WarrantyInfo mappings
            CreateMap<WarrantyInfo, WarrantyInfoDto>();
            CreateMap<CreateWarrantyInfoDto, WarrantyInfo>();
            CreateMap<UpdateWarrantyInfoDto, WarrantyInfo>();
        }
    }
}
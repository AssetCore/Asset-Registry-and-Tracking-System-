using System;
using MaintenanceScheduler.Domain.Entities;

namespace MaintenanceScheduler.Application.DTOs
{
    public class MaintenanceHistoryDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public Guid? MaintenanceScheduleId { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public string? Description { get; set; }
        public string? PartsReplaced { get; set; }
        public decimal Cost { get; set; }
        public string? TechnicianName { get; set; }
        public int DowntimeMinutes { get; set; }
        public string? Notes { get; set; }
        public string? AttachmentUrls { get; set; }
    }

    public class CreateMaintenanceHistoryDto
    {
        public Guid AssetId { get; set; }
        public Guid? MaintenanceScheduleId { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public string? Description { get; set; }
        public string? PartsReplaced { get; set; }
        public decimal Cost { get; set; }
        public string? TechnicianName { get; set; }
        public int DowntimeMinutes { get; set; }
        public string? Notes { get; set; }
        public string? AttachmentUrls { get; set; }
    }
}
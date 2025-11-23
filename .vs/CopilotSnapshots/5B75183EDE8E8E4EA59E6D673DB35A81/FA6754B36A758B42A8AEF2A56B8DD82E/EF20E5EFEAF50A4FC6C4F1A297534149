using System;

namespace MaintenanceScheduler.Domain.Entities
{
    public class MaintenanceHistory
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public Guid? MaintenanceScheduleId { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? PartsReplaced { get; set; }
        public decimal Cost { get; set; }
        public string? TechnicianName { get; set; }
        public int DowntimeMinutes { get; set; }
        public string? Notes { get; set; }
        public string? AttachmentUrls { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        // Navigation property - nullable because MaintenanceScheduleId is nullable
        public MaintenanceSchedule? MaintenanceSchedule { get; set; }
    }
}
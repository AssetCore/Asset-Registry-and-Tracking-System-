using System;
using System.Collections.Generic;

namespace MaintenanceScheduler.Domain.Entities
{
    public class MaintenanceSchedule
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public MaintenanceType MaintenanceType { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public MaintenanceStatus Status { get; set; }
        public int FrequencyInDays { get; set; }
        public DateTime? NextScheduledDate { get; set; }
        public string? AssignedTo { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }

        public ICollection<MaintenanceHistory> MaintenanceHistories { get; set; } = new List<MaintenanceHistory>();
    }

    public enum MaintenanceType
    {
        Preventive = 1,
        Corrective = 2,
        Predictive = 3,
        Routine = 4
    }

    public enum MaintenanceStatus
    {
        Scheduled = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
        Overdue = 5
    }
}
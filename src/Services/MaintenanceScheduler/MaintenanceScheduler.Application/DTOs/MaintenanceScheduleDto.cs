using System;
using MaintenanceScheduler.Domain.Entities;

namespace MaintenanceScheduler.Application.DTOs
{
    public class MaintenanceScheduleDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public string?  AssetName { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public string? Description { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public MaintenanceStatus Status { get; set; }
        public int FrequencyInDays { get; set; }
        public DateTime? NextScheduledDate { get; set; }
        public string? AssignedTo { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateMaintenanceScheduleDto
    {
        public Guid AssetId { get; set; }
        public string? AssetName { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public string? Description { get; set; }
        public DateTime ScheduledDate { get; set; }
        public int FrequencyInDays { get; set; }
        public string? AssignedTo { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateMaintenanceScheduleDto
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public DateTime ScheduledDate { get; set; }
        public MaintenanceStatus Status { get; set; }
        public string? AssignedTo { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public  string? Notes { get; set; }
    }
}
using System;

namespace MaintenanceScheduler.Domain.Entities
{
    public class WarrantyInfo
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string WarrantyProvider { get; set; } = string.Empty;
        public string? WarrantyType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? CoverageDetails { get; set; }
        public decimal WarrantyCost { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public bool IsActive { get; set; }
        public bool NotificationSent { get; set; }
        public DateTime? LastNotificationDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
    }
}

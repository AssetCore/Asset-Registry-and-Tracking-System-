using System;

namespace MaintenanceScheduler.Application.DTOs
{
    public class WarrantyInfoDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public required string AssetName { get; set; }
        public required string WarrantyProvider { get; set; }
        public required string WarrantyType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public required string CoverageDetails { get; set; }
        public decimal WarrantyCost { get; set; }
        public required string ContactPerson { get; set; }
        public required string ContactEmail { get; set; }
        public required string ContactPhone { get; set; }
        public bool IsActive { get; set; }
        public int DaysUntilExpiry { get; set; }
    }

    public class CreateWarrantyInfoDto
    {
        public Guid AssetId { get; set; }
        public required string AssetName { get; set; }
        public required string WarrantyProvider { get; set; }
        public required string WarrantyType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public required string CoverageDetails { get; set; }
        public decimal WarrantyCost { get; set; }
        public required string ContactPerson { get; set; }
        public required string ContactEmail { get; set; }
        public required string ContactPhone { get; set; }
    }

    public class UpdateWarrantyInfoDto
    {
        public Guid Id { get; set; }
        public required string WarrantyProvider { get; set; }
        public required string WarrantyType { get; set; }
        public DateTime ExpiryDate { get; set; }
        public required string CoverageDetails { get; set; }
        public required string ContactPerson { get; set; }
        public required string ContactEmail { get; set; }
        public required string ContactPhone { get; set; }
    }
}
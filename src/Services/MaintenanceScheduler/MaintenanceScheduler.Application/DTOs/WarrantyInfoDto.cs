using System;

namespace MaintenanceScheduler.Application.DTOs
{
    public class WarrantyInfoDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public  string? AssetName { get; set; }
        public  string? WarrantyProvider { get; set; }
        public string? WarrantyType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? CoverageDetails { get; set; }
        public decimal WarrantyCost { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public bool IsActive { get; set; }
        public int DaysUntilExpiry { get; set; }
    }

    public class CreateWarrantyInfoDto
    {
        public Guid AssetId { get; set; }
        public string? AssetName { get; set; }
        public string? WarrantyProvider { get; set; }
        public string? WarrantyType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? CoverageDetails { get; set; }
        public decimal WarrantyCost { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class UpdateWarrantyInfoDto
    {
        public Guid Id { get; set; }
        public string? WarrantyProvider { get; set; }
        public string? WarrantyType { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? CoverageDetails { get; set; }
        public  string? ContactPerson { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }
}
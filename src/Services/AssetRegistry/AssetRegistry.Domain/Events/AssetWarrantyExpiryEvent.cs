namespace AssetRegistry.Domain.Events;

public class AssetWarrantyExpiryEvent
{
    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = default!;
    public string OwnerEmail { get; set; } = default!;
    public string OwnerName { get; set; } = default!;
    public string? SlackChannel { get; set; }
    public DateTime WarrantyExpiryDate { get; set; }
    public int DaysUntilExpiry { get; set; }
}
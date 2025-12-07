namespace AssetRegistry.Domain.Events;

public class AssetMaintenanceDueEvent
{
    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = default!;
    public string OwnerEmail { get; set; } = default!;
    public string OwnerName { get; set; } = default!;
    public string? SlackChannel { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public int DaysUntilMaintenance { get; set; }
}
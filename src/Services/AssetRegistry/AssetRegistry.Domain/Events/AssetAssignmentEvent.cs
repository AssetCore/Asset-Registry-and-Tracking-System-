namespace AssetRegistry.Domain.Events;

public class AssetAssignmentEvent
{
    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = default!;
    public string NewOwnerEmail { get; set; } = default!;
    public string NewOwnerName { get; set; } = default!;
    public string? SlackChannel { get; set; }
    public DateTime AssignmentDate { get; set; }
}
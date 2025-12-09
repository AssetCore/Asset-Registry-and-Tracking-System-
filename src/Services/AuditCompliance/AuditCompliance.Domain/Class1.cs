namespace AuditCompliance.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }

    // User Information
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserRole { get; set; }

    // Action Details
    public string Action { get; set; } = string.Empty;
    public string ActionCategory { get; set; } = string.Empty;

    // Entity Information
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }

    // Change Tracking
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    // Context
    public string? IpAddress { get; set; }
    public string ServiceName { get; set; } = string.Empty;

    // Result
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    // Additional Data
    public string? Metadata { get; set; }
    public string? Reason { get; set; }
}

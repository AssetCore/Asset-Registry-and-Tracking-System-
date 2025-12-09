namespace AuditCompliance.Application.DTOs;

public class AuditLogQueryDto
{
    public string? UserId { get; set; }
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? Success { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

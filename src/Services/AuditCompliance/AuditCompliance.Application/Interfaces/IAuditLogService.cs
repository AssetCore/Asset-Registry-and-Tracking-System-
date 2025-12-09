using AuditCompliance.Application.DTOs;

namespace AuditCompliance.Application.Interfaces;

public interface IAuditLogService
{
    Task<AuditLogDto> CreateAuditLogAsync(CreateAuditLogDto dto, CancellationToken cancellationToken = default);
    Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogQueryDto query, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLogDto>> GetAuditLogsByUserAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLogDto>> GetAuditLogsByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
}

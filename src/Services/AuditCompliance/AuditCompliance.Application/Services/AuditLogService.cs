using AuditCompliance.Application.DTOs;
using AuditCompliance.Application.Interfaces;
using AuditCompliance.Domain.Entities;
using AuditCompliance.Domain.Interfaces;

namespace AuditCompliance.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<AuditLogDto> CreateAuditLogAsync(CreateAuditLogDto dto, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            UserId = dto.UserId,
            UserName = dto.UserName,
            UserRole = dto.UserRole,
            Action = dto.Action,
            ActionCategory = dto.ActionCategory,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            OldValue = dto.OldValue,
            NewValue = dto.NewValue,
            IpAddress = dto.IpAddress,
            ServiceName = dto.ServiceName,
            Success = dto.Success,
            ErrorMessage = dto.ErrorMessage,
            Metadata = dto.Metadata,
            Reason = dto.Reason
        };

        var created = await _repository.CreateAsync(auditLog, cancellationToken);
        return MapToDto(created);
    }

    public async Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var auditLog = await _repository.GetByIdAsync(id, cancellationToken);
        return auditLog == null ? null : MapToDto(auditLog);
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogQueryDto query, CancellationToken cancellationToken = default)
    {
        IEnumerable<AuditLog> logs;

        if (!string.IsNullOrEmpty(query.UserId))
        {
            logs = await _repository.GetByUserIdAsync(query.UserId, query.Page, query.PageSize, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(query.Action))
        {
            logs = await _repository.GetByActionAsync(query.Action, query.Page, query.PageSize, cancellationToken);
        }
        else if (query.StartDate.HasValue && query.EndDate.HasValue)
        {
            logs = await _repository.GetByDateRangeAsync(query.StartDate.Value, query.EndDate.Value, query.Page, query.PageSize, cancellationToken);
        }
        else
        {
            logs = await _repository.GetAllAsync(query.Page, query.PageSize, cancellationToken);
        }

        var totalCount = await _repository.GetTotalCountAsync(cancellationToken);

        return new PagedResult<AuditLogDto>
        {
            Items = logs.Select(MapToDto),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsByUserAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var logs = await _repository.GetByUserIdAsync(userId, page, pageSize, cancellationToken);
        var totalCount = await _repository.GetTotalCountAsync(cancellationToken);

        return new PagedResult<AuditLogDto>
        {
            Items = logs.Select(MapToDto),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
    {
        var logs = await _repository.GetByEntityAsync(entityType, entityId, cancellationToken);
        return logs.Select(MapToDto);
    }

    private static AuditLogDto MapToDto(AuditLog auditLog)
    {
        return new AuditLogDto
        {
            Id = auditLog.Id,
            Timestamp = auditLog.Timestamp,
            UserId = auditLog.UserId,
            UserName = auditLog.UserName,
            UserRole = auditLog.UserRole,
            Action = auditLog.Action,
            ActionCategory = auditLog.ActionCategory,
            EntityType = auditLog.EntityType,
            EntityId = auditLog.EntityId,
            OldValue = auditLog.OldValue,
            NewValue = auditLog.NewValue,
            IpAddress = auditLog.IpAddress,
            ServiceName = auditLog.ServiceName,
            Success = auditLog.Success,
            ErrorMessage = auditLog.ErrorMessage,
            Metadata = auditLog.Metadata,
            Reason = auditLog.Reason
        };
    }
}

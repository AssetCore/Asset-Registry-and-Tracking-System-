using AuditCompliance.Application.DTOs;
using AuditCompliance.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuditCompliance.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(IAuditLogService auditLogService, ILogger<AuditLogsController> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new audit log entry
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogDto dto, CancellationToken cancellationToken)
    {
        try
        {
            // Capture IP address from request
            if (string.IsNullOrEmpty(dto.IpAddress))
            {
                dto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            }

            var result = await _auditLogService.CreateAuditLogAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetAuditLogById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log");
            return StatusCode(500, "An error occurred while creating the audit log");
        }
    }

    /// <summary>
    /// Get audit log by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAuditLogById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _auditLogService.GetAuditLogByIdAsync(id, cancellationToken);

            if (result == null)
            {
                return NotFound($"Audit log with ID {id} not found");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the audit log");
        }
    }

    /// <summary>
    /// Get audit logs with filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogQueryDto query, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _auditLogService.GetAuditLogsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return StatusCode(500, "An error occurred while retrieving audit logs");
        }
    }

    /// <summary>
    /// Get audit logs for a specific user
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetAuditLogsByUser(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _auditLogService.GetAuditLogsByUserAsync(userId, page, pageSize, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving user audit logs");
        }
    }

    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<IActionResult> GetAuditLogsByEntity(string entityType, string entityId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _auditLogService.GetAuditLogsByEntityAsync(entityType, entityId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for entity {EntityType}/{EntityId}", entityType, entityId);
            return StatusCode(500, "An error occurred while retrieving entity audit logs");
        }
    }
}

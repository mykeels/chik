using Microsoft.AspNetCore.Mvc;

namespace Chik.Exams.Api;

[ApiController]
[Route("api/audit-logs")]
[AdminOnly]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Gets an audit log entry by ID. Admin only.
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<AuditLog>> Get(long id, [FromServices] Auth auth)
    {
        var auditLog = await _auditLogService.Get(auth, id);
        if (auditLog is null)
        {
            return NotFound(new { Message = "Audit log not found" });
        }
        return Ok(auditLog);
    }

    /// <summary>
    /// Gets audit logs by service and entity. Admin only.
    /// </summary>
    [HttpGet("by-service/{service}")]
    public async Task<ActionResult<List<AuditLog>>> GetByService(
        string service,
        [FromQuery] long entityId,
        [FromServices] Auth auth)
    {
        var auditLogs = await _auditLogService.GetByService(auth, service, entityId);
        return Ok(auditLogs);
    }

    /// <summary>
    /// Gets audit logs by user. Admin only.
    /// </summary>
    [HttpGet("by-user/{userId:long}")]
    public async Task<ActionResult<List<AuditLog>>> GetByUserId(long userId, [FromServices] Auth auth)
    {
        var auditLogs = await _auditLogService.GetByUserId(auth, userId);
        return Ok(auditLogs);
    }

    /// <summary>
    /// Searches for audit logs. Admin only.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Paginated<AuditLog>>> Search(
        [FromQuery] long? userId,
        [FromQuery] string? service,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] bool includeUser = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromServices] Auth auth = null!)
    {
        var filter = new AuditLog.Filter(
            UserId: userId,
            Service: service,
            DateRange: startDate.HasValue || endDate.HasValue
                ? DateTimeRange.Between(startDate, endDate)
                : null,
            IncludeUser: includeUser ? true : null);

        var pagination = new PaginationOptions(page, pageSize);
        var result = await _auditLogService.Search(auth, filter, pagination);

        return Ok(result);
    }
}

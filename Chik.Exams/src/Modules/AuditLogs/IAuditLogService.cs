using Chik.Exams.Data;

namespace Chik.Exams;

public interface IAuditLogService
{
    public IAuditLogRepository Repository { get; }

    /// <summary>
    /// Creates an audit log entry.
    /// </summary>
    Task<AuditLogDbo> Create<T>(Auth auth, AuditLog.Create<T> auditLog);

    /// <summary>
    /// Gets an audit log entry by ID. Admin only.
    /// </summary>
    Task<AuditLog?> Get(Auth auth, long id);

    /// <summary>
    /// Gets audit logs by service and entity. Admin only.
    /// </summary>
    Task<List<AuditLog>> GetByService(Auth auth, string service, long entityId);

    /// <summary>
    /// Gets audit logs by user. Admin only.
    /// </summary>
    Task<List<AuditLog>> GetByUserId(Auth auth, long userId);

    /// <summary>
    /// Searches for audit logs. Admin only.
    /// </summary>
    Task<Paginated<AuditLog>> Search(Auth auth, AuditLog.Filter? filter = null, PaginationOptions? pagination = null);
}
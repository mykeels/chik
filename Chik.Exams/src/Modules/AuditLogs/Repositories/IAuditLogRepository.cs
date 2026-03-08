namespace Chik.Exams.Data;

public interface IAuditLogRepository
{
    Task<AuditLogDbo> Create(AuditLog.Create auditLog);
    Task<AuditLogDbo?> Get(long id);
    Task<List<AuditLogDbo>> GetByEntity(string entity, long entityId);
    Task<List<AuditLogDbo>> GetByUserId(long userId);
    Task<Paginated<AuditLogDbo>> Search(AuditLog.Filter? filter = null, PaginationOptions? pagination = null);
}

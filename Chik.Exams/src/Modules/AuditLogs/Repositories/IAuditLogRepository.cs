namespace Chik.Exams.Data;

public interface IAuditLogRepository
{
    Task<AuditLogDbo> Create(long actorId, AuditLog.Create auditLog);
    Task<AuditLogDbo?> Get(long id);
    Task<List<AuditLogDbo>> GetByService(string service, long entityId);
    Task<List<AuditLogDbo>> GetByUserId(long userId);
    Task<Paginated<AuditLogDbo>> Search(AuditLog.Filter? filter = null, PaginationOptions? pagination = null);
}

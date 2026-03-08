using Chik.Exams.Data;

namespace Chik.Exams;

public interface IAuditLogService
{
    public IAuditLogRepository Repository { get; }

    Task<AuditLogDbo> Create<T>(Auth auth, AuditLog.Create<T> auditLog);
}
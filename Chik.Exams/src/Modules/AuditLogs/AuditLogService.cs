using Chik.Exams.Data;

namespace Chik.Exams;

internal class AuditLogService(
    IAuditLogRepository repository,
    ILogger<AuditLogService> logger
) : IAuditLogService
{
    public IAuditLogRepository Repository => repository;

    public async Task<AuditLogDbo> Create<T>(Auth auth, AuditLog.Create<T> auditLog)
    {
        logger.LogInformation($"{nameof(AuditLogService)}.{nameof(Create)} ({auth.Id}, {auditLog})");
        return await repository.Create(auth.Id, new AuditLog.Create(auditLog.Service, auditLog.EntityId, Newtonsoft.Json.JsonConvert.SerializeObject(auditLog.Properties)));
    }
}
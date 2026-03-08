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

    public async Task<AuditLog?> Get(Auth auth, long id)
    {
        logger.LogInformation($"{nameof(AuditLogService)}.{nameof(Get)} ({auth.Id}, {id})");
        
        // Authorization: Admin only
        if (!auth.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only Admin can view audit logs");
        }

        var auditLogDbo = await repository.Get(id);
        return auditLogDbo!.ToModel();
    }

    public async Task<List<AuditLog>> GetByService(Auth auth, string service, long entityId)
    {
        logger.LogInformation($"{nameof(AuditLogService)}.{nameof(GetByService)} ({auth.Id}, {service}, {entityId})");
        
        // Authorization: Admin only
        if (!auth.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only Admin can view audit logs");
        }

        var auditLogs = await repository.GetByService(service, entityId);
        return auditLogs.Select(dbo => dbo!.ToModel()).ToList();
    }

    public async Task<List<AuditLog>> GetByUserId(Auth auth, long userId)
    {
        logger.LogInformation($"{nameof(AuditLogService)}.{nameof(GetByUserId)} ({auth.Id}, {userId})");
        
        // Authorization: Admin only
        if (!auth.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only Admin can view audit logs");
        }

        var auditLogs = await repository.GetByUserId(userId);
        return auditLogs.Select(dbo => dbo!.ToModel()).ToList();
    }

    public async Task<Paginated<AuditLog>> Search(Auth auth, AuditLog.Filter? filter = null, PaginationOptions? pagination = null)
    {
        logger.LogInformation($"{nameof(AuditLogService)}.{nameof(Search)} ({auth.Id}, {filter})");
        
        // Authorization: Admin only
        if (!auth.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only Admin can search audit logs");
        }

        filter ??= new AuditLog.Filter();
        pagination ??= new PaginationOptions();

        var paginated = await repository.Search(filter, pagination);
        return new Paginated<AuditLog>(
            paginated.Items.Select(dbo => dbo!.ToModel()).ToList(),
            paginated.TotalCount,
            pagination,
            async options => await Search(auth, filter, options)
        );
    }
}
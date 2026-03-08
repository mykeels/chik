using Microsoft.EntityFrameworkCore;
using Chik.Exams.Data;

namespace Chik.Exams.Data;

public class AuditLogRepository(
    IDbContextFactory<ChikExamsDbContext> _dbContextFactory,
    ILogger<AuditLogRepository> logger,
    TimeProvider timeProvider
) : IAuditLogRepository
{
    public async Task<AuditLogDbo> Create(long actorId, AuditLog.Create auditLog)
    {
        logger.LogInformation($"{nameof(AuditLogRepository)}.{nameof(Create)} (ActorId: {actorId}, Service: {auditLog.Service}, EntityId: {auditLog.EntityId})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var auditLogDbo = new AuditLogDbo
        {
            UserId = actorId,
            Service = auditLog.Service,
            EntityId = auditLog.EntityId,
            Properties = auditLog.Properties,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        };

        await dbContext.AuditLogs.AddAsync(auditLogDbo);
        await dbContext.SaveChangesAsync();
        return auditLogDbo;
    }

    public async Task<AuditLogDbo?> Get(long id)
    {
        logger.LogInformation($"{nameof(AuditLogRepository)}.{nameof(Get)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.AuditLogs.AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<AuditLogDbo>> GetByService(string service, long entityId)
    {
        logger.LogInformation($"{nameof(AuditLogRepository)}.{nameof(GetByService)} ({service}, {entityId})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.AuditLogs.AsNoTracking()
            .Where(a => a.Service == service && a.EntityId == entityId)
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<AuditLogDbo>> GetByUserId(long userId)
    {
        logger.LogInformation($"{nameof(AuditLogRepository)}.{nameof(GetByUserId)} ({userId})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.AuditLogs.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Paginated<AuditLogDbo>> Search(AuditLog.Filter? filter = null, PaginationOptions? pagination = null)
    {
        pagination ??= new PaginationOptions();
        filter ??= new AuditLog.Filter();
        using var dbContext = _dbContextFactory.CreateDbContext();
        var query = GetQuery(dbContext, filter);
        var totalCount = await query.CountAsync();
        var items = await query.Skip(pagination.Skip).Take(pagination.Rows).ToListAsync();
        return new Paginated<AuditLogDbo>(items, totalCount, pagination, async options => await Search(filter, options));
    }

    private IQueryable<AuditLogDbo> GetQuery(ChikExamsDbContext dbContext, AuditLog.Filter filter)
    {
        var query = dbContext.AuditLogs.AsNoTracking();

        if (filter.UserId is not null)
        {
            query = query.Where(a => a.UserId == filter.UserId);
        }

        if (filter.Service is not null)
        {
            query = query.Where(a => a.Service == filter.Service);
        }

        if (filter.EntityIds is not null)
        {
            query = query.Where(a => filter.EntityIds.Contains(a.EntityId));
        }

        if (filter.AuditLogIds is not null && filter.AuditLogIds.Count > 0)
        {
            query = query.Where(a => filter.AuditLogIds.Contains(a.Id));
        }

        if (filter.IncludeUser == true)
        {
            query = query.Include(a => a.User);
        }

        if (filter.DateRange?.From is not null)
        {
            query = query.Where(a => a.CreatedAt >= filter.DateRange.From);
        }

        if (filter.DateRange?.To is not null)
        {
            query = query.Where(a => a.CreatedAt <= filter.DateRange.To);
        }

        query = query.OrderByDescending(a => a.CreatedAt);

        return query;
    }
}

using Microsoft.EntityFrameworkCore;
using Chik.Exams.Data;

namespace Chik.Exams.AuditLogs.Repositories;

public class AuditLogRepository(
    IDbContextFactory<ChikExamsDbContext> _dbContextFactory,
    ILogger<AuditLogRepository> logger,
    TimeProvider timeProvider
) : IAuditLogRepository
{
    public async Task<AuditLogDbo> Create(AuditLog.Create auditLog)
    {
        logger.LogInformation($"{nameof(AuditLogRepository)}.{nameof(Create)} (Entity: {auditLog.Entity}, EntityId: {auditLog.EntityId})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var auditLogDbo = new AuditLogDbo
        {
            UserId = auditLog.UserId,
            Entity = auditLog.Entity,
            EntityId = auditLog.EntityId,
            ApplicationContext = auditLog.ApplicationContext,
            OldValue = auditLog.OldValue,
            NewValue = auditLog.NewValue,
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

    public async Task<List<AuditLogDbo>> GetByEntity(string entity, long entityId)
    {
        logger.LogInformation($"{nameof(AuditLogRepository)}.{nameof(GetByEntity)} ({entity}, {entityId})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.AuditLogs.AsNoTracking()
            .Where(a => a.Entity == entity && a.EntityId == entityId)
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

        if (filter.Entity is not null)
        {
            query = query.Where(a => a.Entity == filter.Entity);
        }

        if (filter.EntityId is not null)
        {
            query = query.Where(a => a.EntityId == filter.EntityId);
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

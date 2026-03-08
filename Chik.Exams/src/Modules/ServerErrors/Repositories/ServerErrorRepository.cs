using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chik.Exams.Data;

public class ServerErrorRepository(
    IDbContextFactory<ChikExamsDbContext> _dbContextFactory,
    ILogger<ServerErrorRepository> logger,
    TimeProvider timeProvider
) : IServerErrorRepository
{
    public async Task<ServerErrorDbo> Create(ServerErrorDbo serverError)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        serverError.CreatedAt = timeProvider.GetUtcNow().DateTime;
        await dbContext.ServerErrors.AddAsync(serverError);
        await dbContext.SaveChangesAsync();
        return serverError;
    }

    public async Task<ServerErrorDbo?> Get(Guid id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.ServerErrors.FindAsync(id);
    }

    public async Task<List<ServerErrorDbo>> Get(ServerError.Filter filter)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var query = GetQuery(dbContext, filter);
        return await query.ToListAsync();
    }

    public async Task<Paginated<ServerErrorDbo>> Search(ServerError.Filter? filter = null, PaginationOptions? pagination = null)
    {
        pagination ??= new PaginationOptions();
        filter ??= new ServerError.Filter();
        using var dbContext = _dbContextFactory.CreateDbContext();
        var query = GetQuery(dbContext, filter);
        var totalCount = await query.CountAsync();
        var items = await query
        .Skip(pagination.Skip)
        .Take(pagination.Rows)
        .ToListAsync();
        return new Paginated<ServerErrorDbo>(items, totalCount, pagination, async options => await Search(filter, options));
    }

    private IQueryable<ServerErrorDbo> GetQuery(ChikExamsDbContext dbContext, ServerError.Filter filter)
    {
        var query = dbContext.ServerErrors.AsQueryable();
        if (filter.UserId is not null)
        {
            query = query.Where(c => c.UserId == filter.UserId);
        }
        if (filter.Text is not null)
        {
            query = query.Where(c => c.Error.Contains(filter.Text));
        }
        if (filter.DateRange?.From is not null)
        {
            query = query.Where(c => c.ErrorAt >= filter.DateRange.From);
        }
        if (filter.DateRange?.To is not null)
        {
            query = query.Where(c => c.ErrorAt <= filter.DateRange.To);
        }
        if (filter.IncludeUser == true)
        {
            query = query.Include(c => c.User);
        }
        query = query.OrderByDescending(c => c.ErrorAt);
        return query;
    }
}
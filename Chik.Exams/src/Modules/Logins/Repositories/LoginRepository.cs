using Microsoft.EntityFrameworkCore;
using Chik.Exams.Data;

namespace Chik.Exams.Logins.Repositories;

public class LoginRepository(
    IDbContextFactory<Chik.ExamsDbContext> _dbContextFactory,
    ILogger<LoginRepository> logger,
    TimeProvider timeProvider
): ILoginRepository
{
    public async Task<LoginDbo> Create(Guid userId, Login.Create login)
    {
        logger.LogInformation($"{nameof(LoginRepository)}.{nameof(Create)} ({userId}, {login})");
        var loginDbo = new LoginDbo
        {
            UserId = userId,
            IpAddressLocationId = login.IpAddressLocationId,
        };
        loginDbo.CreatedAt = timeProvider.GetUtcNow().DateTime;
        using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Logins.AddAsync(loginDbo);
        await dbContext.SaveChangesAsync();
        return loginDbo;
    }

    public async Task<LoginDbo?> GetLastLogin(Guid userId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Logins
            .Where(l => l.UserId == userId)
            .Include(l => l.IpAddressLocation)
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<LoginDbo>> Get(Login.Filter filter)
    {
        filter ??= new Login.Filter();
        using var dbContext = _dbContextFactory.CreateDbContext();
        var query = GetQuery(dbContext, filter);
        return await query.ToListAsync();
    }

    public async Task<Paginated<LoginDbo>> Search(Login.Filter? filter = null, PaginationOptions? pagination = null)
    {
        pagination ??= new PaginationOptions();
        filter ??= new Login.Filter();
        using var dbContext = _dbContextFactory.CreateDbContext();
        var query = GetQuery(dbContext, filter);
        var totalCount = await query.CountAsync();
        var items = await query.Skip(pagination.Skip).Take(pagination.Rows).ToListAsync();
        return new Paginated<LoginDbo>(items, totalCount, pagination, async options => await Search(filter, options));
    }

    private IQueryable<LoginDbo> GetQuery(Chik.ExamsDbContext dbContext, Login.Filter filter)
    {
        logger.LogInformation($"{nameof(LoginRepository)}.{nameof(GetQuery)} ({filter})");
        var query = dbContext.Logins.AsNoTracking();
        if (filter?.UserId is not null)
        {
            query = query.Where(l => l.UserId == filter.UserId);
        }

        if (filter?.IpAddressLocationId is not null)
        {
            query = query.Where(l => l.IpAddressLocationId == filter.IpAddressLocationId);
        }

        if (filter?.IpAddress is not null)
        {
            query = query.Where(l => l.IpAddressLocation!.IpAddress == filter.IpAddress);
        }

        if (filter?.CountryCode is not null)
        {
            query = query.Where(l => l.IpAddressLocation!.CountryCode == filter.CountryCode);
        }

        if (filter?.IncludeUser == true)
        {
            query = query.Include(l => l.User);
        }

        if (filter?.IncludeIpAddressLocation == true)
        {
            query = query.Include(l => l.IpAddressLocation);
        }

        if (filter?.DateRange?.From is not null)
        {
            query = query.Where(l => l.CreatedAt >= filter.DateRange.From);
        }
        
        if (filter?.DateRange?.To is not null)
        {
            query = query.Where(l => l.CreatedAt <= filter.DateRange.To);
        }

        query = query.OrderByDescending(l => l.CreatedAt);

        return query;
    }
}
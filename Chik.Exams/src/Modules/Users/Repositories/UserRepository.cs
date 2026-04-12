using Microsoft.EntityFrameworkCore;
using Chik.Exams.Data;

namespace Chik.Exams.Data;

public class UserRepository(
    IDbContextFactory<ChikExamsDbContext> _dbContextFactory,
    ILogger<UserRepository> logger,
    TimeProvider timeProvider
) : IUserRepository
{
    public async Task<UserDbo> Create(User.Create user)
    {
        logger.LogInformation($"{nameof(UserRepository)}.{nameof(Create)} ({user.Username})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
        if (existingUser is not null)
        {
            throw new InvalidOperationException($"User with username '{user.Username}' already exists");
        }

        var hasStudent = user.Roles.Any(r => r == UserRole.Student);
        var hasTeacher = user.Roles.Any(r => r == UserRole.Teacher);

        var userDbo = new UserDbo
        {
            Username = user.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
            Roles = user.Roles.ToInt32(),
            CreatedAt = timeProvider.GetUtcNow().DateTime
        };

        await dbContext.Users.AddAsync(userDbo);
        await dbContext.SaveChangesAsync();

        if (hasStudent && user.ClassId is not null)
        {
            await dbContext.UserClasses.AddAsync(new UserClassDbo
            {
                UserId = userDbo.Id,
                ClassId = user.ClassId.Value,
                CreatedAt = timeProvider.GetUtcNow().DateTime
            });
            await dbContext.SaveChangesAsync();
        }

        if (hasTeacher && user.ClassIds is not null)
        {
            foreach (var classId in user.ClassIds.Distinct())
            {
                await dbContext.UserClasses.AddAsync(new UserClassDbo
                {
                    UserId = userDbo.Id,
                    ClassId = classId,
                    CreatedAt = timeProvider.GetUtcNow().DateTime
                });
            }
            await dbContext.SaveChangesAsync();
        }

        return await Get(userDbo.Id) ?? userDbo;
    }

    public async Task<UserDbo?> Get(long id)
    {
        logger.LogInformation($"{nameof(UserRepository)}.{nameof(Get)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Users.AsNoTracking()
            .Include(u => u.UserClasses)!.ThenInclude(uc => uc.Class)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserDbo?> Get(string username)
    {
        logger.LogInformation($"{nameof(UserRepository)}.{nameof(Get)} ({username})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Users.AsNoTracking()
            .Include(u => u.UserClasses)!.ThenInclude(uc => uc.Class)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<UserDbo> Update(long id, User.Update user)
    {
        logger.LogInformation($"{nameof(UserRepository)}.{nameof(Update)} ({id}, {user})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existingUser = await dbContext.Users
            .Include(u => u.UserClasses)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (existingUser is null)
        {
            throw new KeyNotFoundException($"User with id '{id}' not found");
        }

        if (user.Username is not null)
        {
            existingUser.Username = user.Username;
        }
        if (user.Password is not null)
        {
            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        }
        if (user.Roles is not null)
        {
            existingUser.Roles = user.Roles.ToInt32();
        }

        if (user.ClassId is not null)
        {
            if ((existingUser.Roles & (int)UserRole.Student) == 0)
                throw new InvalidOperationException("ClassId can only be set for users with the Student role");
            dbContext.UserClasses.RemoveRange(existingUser.UserClasses ?? []);
            await dbContext.UserClasses.AddAsync(new UserClassDbo
            {
                UserId = id,
                ClassId = user.ClassId.Value,
                CreatedAt = timeProvider.GetUtcNow().DateTime
            });
        }
        else if (user.ClassIds is not null)
        {
            if ((existingUser.Roles & (int)UserRole.Teacher) == 0)
                throw new InvalidOperationException("ClassIds can only be set for users with the Teacher role");
            dbContext.UserClasses.RemoveRange(existingUser.UserClasses ?? []);
            foreach (var classId in user.ClassIds.Distinct())
            {
                await dbContext.UserClasses.AddAsync(new UserClassDbo
                {
                    UserId = id,
                    ClassId = classId,
                    CreatedAt = timeProvider.GetUtcNow().DateTime
                });
            }
        }

        existingUser.UpdatedAt = timeProvider.GetUtcNow().DateTime;

        await dbContext.SaveChangesAsync();
        return await Get(id) ?? existingUser;
    }

    public async Task<Paginated<UserDbo>> Search(User.Filter? filter = null, PaginationOptions? pagination = null)
    {
        pagination ??= new PaginationOptions();
        filter ??= new User.Filter();
        using var dbContext = _dbContextFactory.CreateDbContext();
        var query = GetQuery(dbContext, filter);
        var totalCount = await query.CountAsync();
        var items = await query.Skip(pagination.Skip).Take(pagination.Rows).ToListAsync();
        return new Paginated<UserDbo>(items, totalCount, pagination, async options => await Search(filter, options));
    }

    public async Task Delete(long id)
    {
        logger.LogInformation($"{nameof(UserRepository)}.{nameof(Delete)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return;
        }
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<long>> GetStudentUserIdsInClass(int classId)
    {
        logger.LogInformation($"{nameof(UserRepository)}.{nameof(GetStudentUserIdsInClass)} ({classId})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.UserClasses.AsNoTracking()
            .Where(uc => uc.ClassId == classId)
            .Join(
                dbContext.Users.AsNoTracking(),
                uc => uc.UserId,
                u => u.Id,
                (uc, u) => u)
            .Where(u => (u.Roles & (int)UserRole.Student) != 0)
            .Select(u => u.Id)
            .Distinct()
            .ToListAsync();
    }

    public async Task<int?> GetStudentClassIdForUser(long userId)
    {
        logger.LogInformation($"{nameof(UserRepository)}.{nameof(GetStudentClassIdForUser)} ({userId})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        var user = await dbContext.Users.AsNoTracking()
            .Include(u => u.UserClasses)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null || (user.Roles & (int)UserRole.Student) == 0)
            return null;
        return user.UserClasses?.OrderBy(uc => uc.Id).FirstOrDefault()?.ClassId;
    }

    private IQueryable<UserDbo> GetQuery(ChikExamsDbContext dbContext, User.Filter filter)
    {
        IQueryable<UserDbo> query = dbContext.Users.AsNoTracking()
            .Include(u => u.UserClasses)!.ThenInclude(uc => uc.Class);

        if (filter.Username is not null)
            query = query.Where(u => u.Username.Contains(filter.Username));

        if (filter.Roles is not null)
            query = query.Where(u => (u.Roles & filter.Roles.Value) != 0);

        if (filter.UserIds is not null && filter.UserIds.Count > 0)
            query = query.Where(u => filter.UserIds.Contains(u.Id));

        if (filter.DateRange?.From is not null)
            query = query.Where(u => u.CreatedAt >= filter.DateRange.From);

        if (filter.DateRange?.To is not null)
            query = query.Where(u => u.CreatedAt <= filter.DateRange.To);

        return query.OrderByDescending(u => u.CreatedAt);
    }
}

using Microsoft.EntityFrameworkCore;

namespace Chik.Exams.Data;

public class ClassRepository(
    IDbContextFactory<ChikExamsDbContext> dbContextFactory,
    ILogger<ClassRepository> logger,
    TimeProvider timeProvider
) : IClassRepository
{
    public async Task<ClassDbo?> Get(int id)
    {
        logger.LogInformation($"{nameof(ClassRepository)}.{nameof(Get)} ({id})");
        using var db = dbContextFactory.CreateDbContext();
        return await db.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<ClassDbo>> List()
    {
        logger.LogInformation($"{nameof(ClassRepository)}.{nameof(List)}");
        using var db = dbContextFactory.CreateDbContext();
        return await db.Classes.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<ClassDbo> Create(Class.Create create)
    {
        logger.LogInformation($"{nameof(ClassRepository)}.{nameof(Create)} ({create.Name})");
        using var db = dbContextFactory.CreateDbContext();
        var dbo = new ClassDbo
        {
            Name = create.Name.Trim(),
            CreatedAt = timeProvider.GetUtcNow().DateTime
        };
        await db.Classes.AddAsync(dbo);
        await db.SaveChangesAsync();
        return dbo;
    }

    public async Task<List<int>> GetClassIdsForTeacher(long userId)
    {
        using var db = dbContextFactory.CreateDbContext();
        return await db.UserClasses.AsNoTracking()
            .Where(uc => uc.UserId == userId)
            .Select(uc => uc.ClassId)
            .ToListAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using Chik.Exams.Data;

namespace Chik.Exams.Quizzes.QuestionTypes.Repositories;

public class QuizQuestionTypeRepository(
    IDbContextFactory<ChikExamsDbContext> _dbContextFactory,
    ILogger<QuizQuestionTypeRepository> logger,
    TimeProvider timeProvider
) : IQuizQuestionTypeRepository
{
    public async Task<QuizQuestionTypeDbo> Create(QuizQuestionType.Create type)
    {
        logger.LogInformation($"{nameof(QuizQuestionTypeRepository)}.{nameof(Create)} ({type.Name})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existingType = await dbContext.QuizQuestionTypes.FirstOrDefaultAsync(t => t.Name == type.Name);
        if (existingType is not null)
        {
            throw new InvalidOperationException($"Question type with name '{type.Name}' already exists");
        }

        var typeDbo = new QuizQuestionTypeDbo
        {
            Name = type.Name,
            Description = type.Description,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        };

        await dbContext.QuizQuestionTypes.AddAsync(typeDbo);
        await dbContext.SaveChangesAsync();
        return typeDbo;
    }

    public async Task<QuizQuestionTypeDbo?> Get(long id)
    {
        logger.LogInformation($"{nameof(QuizQuestionTypeRepository)}.{nameof(Get)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.QuizQuestionTypes.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<QuizQuestionTypeDbo?> GetByName(string name)
    {
        logger.LogInformation($"{nameof(QuizQuestionTypeRepository)}.{nameof(GetByName)} ({name})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.QuizQuestionTypes.AsNoTracking().FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<List<QuizQuestionTypeDbo>> GetAll()
    {
        logger.LogInformation($"{nameof(QuizQuestionTypeRepository)}.{nameof(GetAll)}");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.QuizQuestionTypes.AsNoTracking().OrderBy(t => t.Id).ToListAsync();
    }

    public async Task<QuizQuestionTypeDbo> Update(long id, QuizQuestionType.Update type)
    {
        logger.LogInformation($"{nameof(QuizQuestionTypeRepository)}.{nameof(Update)} ({id}, {type})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existingType = await dbContext.QuizQuestionTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (existingType is null)
        {
            throw new KeyNotFoundException($"Question type with id '{id}' not found");
        }

        if (type.Name is not null)
        {
            existingType.Name = type.Name;
        }
        if (type.Description is not null)
        {
            existingType.Description = type.Description;
        }
        existingType.UpdatedAt = timeProvider.GetUtcNow().DateTime;

        await dbContext.SaveChangesAsync();
        return existingType;
    }

    public async Task<Paginated<QuizQuestionTypeDbo>> Search(QuizQuestionType.Filter? filter = null, PaginationOptions? pagination = null)
    {
        pagination ??= new PaginationOptions();
        filter ??= new QuizQuestionType.Filter();
        using var dbContext = _dbContextFactory.CreateDbContext();
        var query = GetQuery(dbContext, filter);
        var totalCount = await query.CountAsync();
        var items = await query.Skip(pagination.Skip).Take(pagination.Rows).ToListAsync();
        return new Paginated<QuizQuestionTypeDbo>(items, totalCount, pagination, async options => await Search(filter, options));
    }

    public async Task Delete(long id)
    {
        logger.LogInformation($"{nameof(QuizQuestionTypeRepository)}.{nameof(Delete)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        var type = await dbContext.QuizQuestionTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (type is null)
        {
            return;
        }
        dbContext.QuizQuestionTypes.Remove(type);
        await dbContext.SaveChangesAsync();
    }

    private IQueryable<QuizQuestionTypeDbo> GetQuery(ChikExamsDbContext dbContext, QuizQuestionType.Filter filter)
    {
        var query = dbContext.QuizQuestionTypes.AsNoTracking();

        if (filter.Name is not null)
        {
            query = query.Where(t => t.Name.Contains(filter.Name));
        }

        if (filter.TypeIds is not null && filter.TypeIds.Count > 0)
        {
            query = query.Where(t => filter.TypeIds.Contains(t.Id));
        }

        if (filter.IncludeQuestions == true)
        {
            query = query.Include(t => t.Questions);
        }

        if (filter.DateRange?.From is not null)
        {
            query = query.Where(t => t.CreatedAt >= filter.DateRange.From);
        }

        if (filter.DateRange?.To is not null)
        {
            query = query.Where(t => t.CreatedAt <= filter.DateRange.To);
        }

        query = query.OrderBy(t => t.Id);

        return query;
    }
}

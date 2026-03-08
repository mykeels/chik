using Microsoft.EntityFrameworkCore;
using Chik.Exams.Data;

namespace Chik.Exams.Data;

public class QuizRepository(
    IDbContextFactory<ChikExamsDbContext> _dbContextFactory,
    ILogger<QuizRepository> logger,
    TimeProvider timeProvider
) : IQuizRepository
{
    public async Task<QuizDbo> Create(Quiz.Create quiz)
    {
        logger.LogInformation($"{nameof(QuizRepository)}.{nameof(Create)} ({quiz.Title})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var quizDbo = new QuizDbo
        {
            Title = quiz.Title,
            Description = quiz.Description,
            CreatorId = quiz.CreatorId,
            ExaminerId = quiz.ExaminerId,
            Duration = quiz.Duration,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        };

        await dbContext.Quizzes.AddAsync(quizDbo);
        await dbContext.SaveChangesAsync();
        return quizDbo;
    }

    public async Task<QuizDbo?> Get(long id, bool includeQuestions = false)
    {
        logger.LogInformation($"{nameof(QuizRepository)}.{nameof(Get)} ({id}, includeQuestions: {includeQuestions})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        
        var query = dbContext.Quizzes.AsNoTracking();
        
        if (includeQuestions)
        {
            query = query.Include(q => q.Questions!.Where(qq => qq.DeactivatedAt == null).OrderBy(qq => qq.Order));
        }

        return await query.FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<QuizDbo> Update(long id, Quiz.Update quiz)
    {
        logger.LogInformation($"{nameof(QuizRepository)}.{nameof(Update)} ({id}, {quiz})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existingQuiz = await dbContext.Quizzes.FirstOrDefaultAsync(q => q.Id == id);
        if (existingQuiz is null)
        {
            throw new KeyNotFoundException($"Quiz with id '{id}' not found");
        }

        if (quiz.Title is not null)
        {
            existingQuiz.Title = quiz.Title;
        }
        if (quiz.Description is not null)
        {
            existingQuiz.Description = quiz.Description;
        }
        if (quiz.ExaminerId is not null)
        {
            existingQuiz.ExaminerId = quiz.ExaminerId;
        }
        if (quiz.Duration is not null)
        {
            existingQuiz.Duration = quiz.Duration;
        }
        existingQuiz.UpdatedAt = timeProvider.GetUtcNow().DateTime;

        await dbContext.SaveChangesAsync();
        return existingQuiz;
    }

    public async Task<Paginated<QuizDbo>> Search(Quiz.Filter? filter = null, PaginationOptions? pagination = null)
    {
        pagination ??= new PaginationOptions();
        filter ??= new Quiz.Filter();
        using var dbContext = _dbContextFactory.CreateDbContext();
        var query = GetQuery(dbContext, filter);
        var totalCount = await query.CountAsync();
        var items = await query.Skip(pagination.Skip).Take(pagination.Rows).ToListAsync();
        return new Paginated<QuizDbo>(items, totalCount, pagination, async options => await Search(filter, options));
    }

    public async Task Delete(long id)
    {
        logger.LogInformation($"{nameof(QuizRepository)}.{nameof(Delete)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        var quiz = await dbContext.Quizzes.FirstOrDefaultAsync(q => q.Id == id);
        if (quiz is null)
        {
            return;
        }
        dbContext.Quizzes.Remove(quiz);
        await dbContext.SaveChangesAsync();
    }

    private IQueryable<QuizDbo> GetQuery(ChikExamsDbContext dbContext, Quiz.Filter filter)
    {
        var query = dbContext.Quizzes.AsNoTracking();

        if (filter.Title is not null)
        {
            query = query.Where(q => q.Title.Contains(filter.Title));
        }

        if (filter.CreatorId is not null)
        {
            query = query.Where(q => q.CreatorId == filter.CreatorId);
        }

        if (filter.ExaminerId is not null)
        {
            query = query.Where(q => q.ExaminerId == filter.ExaminerId);
        }

        if (filter.QuizIds is not null && filter.QuizIds.Count > 0)
        {
            query = query.Where(q => filter.QuizIds.Contains(q.Id));
        }

        if (filter.IncludeCreator == true)
        {
            query = query.Include(q => q.Creator);
        }

        if (filter.IncludeExaminer == true)
        {
            query = query.Include(q => q.Examiner);
        }

        if (filter.IncludeQuestions == true)
        {
            query = query.Include(q => q.Questions!.Where(qq => qq.DeactivatedAt == null).OrderBy(qq => qq.Order));
        }

        if (filter.DateRange?.From is not null)
        {
            query = query.Where(q => q.CreatedAt >= filter.DateRange.From);
        }

        if (filter.DateRange?.To is not null)
        {
            query = query.Where(q => q.CreatedAt <= filter.DateRange.To);
        }

        query = query.OrderByDescending(q => q.CreatedAt);

        return query;
    }
}

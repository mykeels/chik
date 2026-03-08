using Microsoft.EntityFrameworkCore;
using Chik.Exams.Data;

namespace Chik.Exams.Quizzes.Questions.Repositories;

public class QuizQuestionRepository(
    IDbContextFactory<ChikExamsDbContext> _dbContextFactory,
    ILogger<QuizQuestionRepository> logger,
    TimeProvider timeProvider
) : IQuizQuestionRepository
{
    public async Task<QuizQuestionDbo> Create(QuizQuestion.Create question)
    {
        logger.LogInformation($"{nameof(QuizQuestionRepository)}.{nameof(Create)} (QuizId: {question.QuizId})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var questionDbo = new QuizQuestionDbo
        {
            QuizId = question.QuizId,
            Prompt = question.Prompt,
            TypeId = (int)question.TypeId,
            Properties = question.Properties,
            Score = question.Score,
            Order = question.Order,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        };

        await dbContext.QuizQuestions.AddAsync(questionDbo);
        await dbContext.SaveChangesAsync();
        return questionDbo;
    }

    public async Task<QuizQuestionDbo?> Get(long id)
    {
        logger.LogInformation($"{nameof(QuizQuestionRepository)}.{nameof(Get)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.QuizQuestions.AsNoTracking()
            .Include(q => q.Type)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<List<QuizQuestionDbo>> GetByQuizId(long quizId, bool includeDeactivated = false)
    {
        logger.LogInformation($"{nameof(QuizQuestionRepository)}.{nameof(GetByQuizId)} ({quizId}, includeDeactivated: {includeDeactivated})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        
        var query = dbContext.QuizQuestions.AsNoTracking()
            .Where(q => q.QuizId == quizId);

        if (!includeDeactivated)
        {
            query = query.Where(q => q.DeactivatedAt == null);
        }

        return await query.OrderBy(q => q.Order).ToListAsync();
    }

    public async Task<QuizQuestionDbo> Update(long id, QuizQuestion.Update question)
    {
        logger.LogInformation($"{nameof(QuizQuestionRepository)}.{nameof(Update)} ({id}, {question})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existingQuestion = await dbContext.QuizQuestions.FirstOrDefaultAsync(q => q.Id == id);
        if (existingQuestion is null)
        {
            throw new KeyNotFoundException($"Question with id '{id}' not found");
        }

        if (question.Prompt is not null)
        {
            existingQuestion.Prompt = question.Prompt;
        }
        if (question.TypeId is not null)
        {
            existingQuestion.TypeId = (int)question.TypeId.Value;
        }
        if (question.Properties is not null)
        {
            existingQuestion.Properties = question.Properties;
        }
        if (question.Score is not null)
        {
            existingQuestion.Score = question.Score.Value;
        }
        if (question.Order is not null)
        {
            existingQuestion.Order = question.Order.Value;
        }
        if (question.DeactivatedAt is not null)
        {
            existingQuestion.DeactivatedAt = question.DeactivatedAt;
        }
        existingQuestion.UpdatedAt = timeProvider.GetUtcNow().DateTime;

        await dbContext.SaveChangesAsync();
        return existingQuestion;
    }

    public async Task<Paginated<QuizQuestionDbo>> Search(QuizQuestion.Filter? filter = null, PaginationOptions? pagination = null)
    {
        pagination ??= new PaginationOptions();
        filter ??= new QuizQuestion.Filter();
        using var dbContext = _dbContextFactory.CreateDbContext();
        var query = GetQuery(dbContext, filter);
        var totalCount = await query.CountAsync();
        var items = await query.Skip(pagination.Skip).Take(pagination.Rows).ToListAsync();
        return new Paginated<QuizQuestionDbo>(items, totalCount, pagination, async options => await Search(filter, options));
    }

    public async Task Deactivate(long id)
    {
        logger.LogInformation($"{nameof(QuizQuestionRepository)}.{nameof(Deactivate)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var question = await dbContext.QuizQuestions.FirstOrDefaultAsync(q => q.Id == id);
        if (question is null)
        {
            throw new KeyNotFoundException($"Question with id '{id}' not found");
        }

        question.DeactivatedAt = timeProvider.GetUtcNow().DateTime;
        question.UpdatedAt = timeProvider.GetUtcNow().DateTime;
        await dbContext.SaveChangesAsync();
    }

    public async Task Reactivate(long id)
    {
        logger.LogInformation($"{nameof(QuizQuestionRepository)}.{nameof(Reactivate)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var question = await dbContext.QuizQuestions.FirstOrDefaultAsync(q => q.Id == id);
        if (question is null)
        {
            throw new KeyNotFoundException($"Question with id '{id}' not found");
        }

        question.DeactivatedAt = null;
        question.UpdatedAt = timeProvider.GetUtcNow().DateTime;
        await dbContext.SaveChangesAsync();
    }

    public async Task Delete(long id)
    {
        logger.LogInformation($"{nameof(QuizQuestionRepository)}.{nameof(Delete)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        var question = await dbContext.QuizQuestions.FirstOrDefaultAsync(q => q.Id == id);
        if (question is null)
        {
            return;
        }
        dbContext.QuizQuestions.Remove(question);
        await dbContext.SaveChangesAsync();
    }

    public async Task ReorderQuestions(long quizId, List<long> questionIdsInOrder)
    {
        logger.LogInformation($"{nameof(QuizQuestionRepository)}.{nameof(ReorderQuestions)} ({quizId}, {questionIdsInOrder.Count} questions)");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var questions = await dbContext.QuizQuestions
            .Where(q => q.QuizId == quizId && questionIdsInOrder.Contains(q.Id))
            .ToListAsync();

        for (int i = 0; i < questionIdsInOrder.Count; i++)
        {
            var question = questions.FirstOrDefault(q => q.Id == questionIdsInOrder[i]);
            if (question is not null)
            {
                question.Order = i + 1;
                question.UpdatedAt = timeProvider.GetUtcNow().DateTime;
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private IQueryable<QuizQuestionDbo> GetQuery(ChikExamsDbContext dbContext, QuizQuestion.Filter filter)
    {
        var query = dbContext.QuizQuestions.AsNoTracking();

        if (filter.QuizId is not null)
        {
            query = query.Where(q => q.QuizId == filter.QuizId);
        }

        if (filter.TypeId is not null)
        {
            query = query.Where(q => q.TypeId == filter.TypeId);
        }

        if (filter.IsActive is not null)
        {
            if (filter.IsActive.Value)
            {
                query = query.Where(q => q.DeactivatedAt == null);
            }
            else
            {
                query = query.Where(q => q.DeactivatedAt != null);
            }
        }

        if (filter.QuestionIds is not null && filter.QuestionIds.Count > 0)
        {
            query = query.Where(q => filter.QuestionIds.Contains(q.Id));
        }

        if (filter.IncludeQuiz == true)
        {
            query = query.Include(q => q.Quiz);
        }

        if (filter.IncludeType == true)
        {
            query = query.Include(q => q.Type);
        }

        if (filter.DateRange?.From is not null)
        {
            query = query.Where(q => q.CreatedAt >= filter.DateRange.From);
        }

        if (filter.DateRange?.To is not null)
        {
            query = query.Where(q => q.CreatedAt <= filter.DateRange.To);
        }

        query = query.OrderBy(q => q.Order);

        return query;
    }
}

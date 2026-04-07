using Microsoft.EntityFrameworkCore;
using Chik.Exams.Data;

namespace Chik.Exams.Data;

public class ExamRepository(
    IDbContextFactory<ChikExamsDbContext> _dbContextFactory,
    ILogger<ExamRepository> logger,
    TimeProvider timeProvider
) : IExamRepository
{
    public async Task<ExamDbo> Create(Exam.Create exam)
    {
        logger.LogInformation($"{nameof(ExamRepository)}.{nameof(Create)} (UserId: {exam.UserId}, QuizId: {exam.QuizId})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var examDbo = new ExamDbo
        {
            UserId = exam.UserId,
            QuizId = exam.QuizId,
            CreatorId = exam.CreatorId,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        };

        await dbContext.Exams.AddAsync(examDbo);
        await dbContext.SaveChangesAsync();
        return examDbo;
    }

    public async Task<ExamDbo?> Get(long id, bool includeAnswers = false)
    {
        logger.LogInformation($"{nameof(ExamRepository)}.{nameof(Get)} ({id}, includeAnswers: {includeAnswers})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        IQueryable<ExamDbo> query = dbContext.Exams.AsNoTracking()
            .Include(e => e.Quiz)
            .Include(e => e.User);

        if (includeAnswers)
        {
            query = query.Include(e => e.Answers);
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<bool> UserHasAssignedExamForQuiz(long userId, long quizId)
    {
        logger.LogInformation($"{nameof(ExamRepository)}.{nameof(UserHasAssignedExamForQuiz)} ({userId}, {quizId})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Exams.AsNoTracking()
            .AnyAsync(e => e.UserId == userId && e.QuizId == quizId);
    }

    public async Task<List<ExamDbo>> GetByUserId(long userId)
    {
        logger.LogInformation($"{nameof(ExamRepository)}.{nameof(GetByUserId)} ({userId})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Exams.AsNoTracking()
            .Where(e => e.UserId == userId)
            .Include(e => e.Quiz)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ExamDbo>> GetByQuizId(long quizId)
    {
        logger.LogInformation($"{nameof(ExamRepository)}.{nameof(GetByQuizId)} ({quizId})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Exams.AsNoTracking()
            .Where(e => e.QuizId == quizId)
            .Include(e => e.User)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<ExamDbo> Update(long id, Exam.Update exam)
    {
        logger.LogInformation($"{nameof(ExamRepository)}.{nameof(Update)} ({id}, {exam})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existingExam = await dbContext.Exams.FirstOrDefaultAsync(e => e.Id == id);
        if (existingExam is null)
        {
            throw new KeyNotFoundException($"Exam with id '{id}' not found");
        }

        if (exam.StartedAt is not null)
        {
            existingExam.StartedAt = exam.StartedAt;
        }
        if (exam.EndedAt is not null)
        {
            existingExam.EndedAt = exam.EndedAt;
        }
        if (exam.Score is not null)
        {
            existingExam.Score = exam.Score;
        }
        if (exam.ExaminerId is not null)
        {
            existingExam.ExaminerId = exam.ExaminerId;
        }
        if (exam.ExaminerComment is not null)
        {
            existingExam.ExaminerComment = exam.ExaminerComment;
        }
        existingExam.UpdatedAt = timeProvider.GetUtcNow().DateTime;

        await dbContext.SaveChangesAsync();
        return existingExam;
    }

    public async Task<Paginated<ExamDbo>> Search(Exam.Filter? filter = null, PaginationOptions? pagination = null)
    {
        pagination ??= new PaginationOptions();
        filter ??= new Exam.Filter();
        using var dbContext = _dbContextFactory.CreateDbContext();
        var query = GetQuery(dbContext, filter);
        var totalCount = await query.CountAsync();
        var items = await query.Skip(pagination.Skip).Take(pagination.Rows).ToListAsync();
        return new Paginated<ExamDbo>(items, totalCount, pagination, async options => await Search(filter, options));
    }

    public async Task<ExamDbo> Start(long id)
    {
        logger.LogInformation($"{nameof(ExamRepository)}.{nameof(Start)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var exam = await dbContext.Exams.FirstOrDefaultAsync(e => e.Id == id);
        if (exam is null)
        {
            throw new KeyNotFoundException($"Exam with id '{id}' not found");
        }

        if (exam.StartedAt is not null)
        {
            throw new InvalidOperationException($"Exam with id '{id}' has already been started");
        }

        exam.StartedAt = timeProvider.GetUtcNow().DateTime;
        exam.UpdatedAt = timeProvider.GetUtcNow().DateTime;
        await dbContext.SaveChangesAsync();
        return exam;
    }

    public async Task<ExamDbo> End(long id)
    {
        logger.LogInformation($"{nameof(ExamRepository)}.{nameof(End)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var exam = await dbContext.Exams.FirstOrDefaultAsync(e => e.Id == id);
        if (exam is null)
        {
            throw new KeyNotFoundException($"Exam with id '{id}' not found");
        }

        if (exam.StartedAt is null)
        {
            throw new InvalidOperationException($"Exam with id '{id}' has not been started");
        }

        if (exam.EndedAt is not null)
        {
            throw new InvalidOperationException($"Exam with id '{id}' has already been ended");
        }

        exam.EndedAt = timeProvider.GetUtcNow().DateTime;
        exam.UpdatedAt = timeProvider.GetUtcNow().DateTime;
        await dbContext.SaveChangesAsync();
        return exam;
    }

    public async Task<ExamDbo> Mark(long id, int score, long examinerId, string? comment = null)
    {
        logger.LogInformation($"{nameof(ExamRepository)}.{nameof(Mark)} ({id}, score: {score}, examinerId: {examinerId})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var exam = await dbContext.Exams.FirstOrDefaultAsync(e => e.Id == id);
        if (exam is null)
        {
            throw new KeyNotFoundException($"Exam with id '{id}' not found");
        }

        exam.Score = score;
        exam.ExaminerId = examinerId;
        exam.ExaminerComment = comment;
        exam.UpdatedAt = timeProvider.GetUtcNow().DateTime;
        await dbContext.SaveChangesAsync();
        return exam;
    }

    public async Task Delete(long id)
    {
        logger.LogInformation($"{nameof(ExamRepository)}.{nameof(Delete)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        var exam = await dbContext.Exams.FirstOrDefaultAsync(e => e.Id == id);
        if (exam is null)
        {
            return;
        }
        dbContext.Exams.Remove(exam);
        await dbContext.SaveChangesAsync();
    }

    private IQueryable<ExamDbo> GetQuery(ChikExamsDbContext dbContext, Exam.Filter filter)
    {
        var query = dbContext.Exams.AsNoTracking();

        if (filter.UserId is not null)
        {
            query = query.Where(e => e.UserId == filter.UserId);
        }

        if (filter.QuizId is not null)
        {
            query = query.Where(e => e.QuizId == filter.QuizId);
        }

        if (filter.CreatorId is not null)
        {
            query = query.Where(e => e.CreatorId == filter.CreatorId);
        }

        if (filter.ExaminerId is not null)
        {
            query = query.Where(e => e.ExaminerId == filter.ExaminerId);
        }

        if (filter.IsStarted is not null)
        {
            if (filter.IsStarted.Value)
            {
                query = query.Where(e => e.StartedAt != null);
            }
            else
            {
                query = query.Where(e => e.StartedAt == null);
            }
        }

        if (filter.IsEnded is not null)
        {
            if (filter.IsEnded.Value)
            {
                query = query.Where(e => e.EndedAt != null);
            }
            else
            {
                query = query.Where(e => e.EndedAt == null);
            }
        }

        if (filter.IsMarked is not null)
        {
            if (filter.IsMarked.Value)
            {
                query = query.Where(e => e.Score != null);
            }
            else
            {
                query = query.Where(e => e.Score == null);
            }
        }

        if (filter.ExamIds is not null && filter.ExamIds.Count > 0)
        {
            query = query.Where(e => filter.ExamIds.Contains(e.Id));
        }

        if (filter.IncludeUser == true)
        {
            query = query.Include(e => e.User);
        }

        if (filter.IncludeQuiz == true)
        {
            query = query.Include(e => e.Quiz);
        }

        if (filter.IncludeCreator == true)
        {
            query = query.Include(e => e.Creator);
        }

        if (filter.IncludeExaminer == true)
        {
            query = query.Include(e => e.Examiner);
        }

        if (filter.IncludeAnswers == true)
        {
            query = query.Include(e => e.Answers);
        }

        if (filter.DateRange?.From is not null)
        {
            query = query.Where(e => e.CreatedAt >= filter.DateRange.From);
        }

        if (filter.DateRange?.To is not null)
        {
            query = query.Where(e => e.CreatedAt <= filter.DateRange.To);
        }

        query = query.OrderByDescending(e => e.CreatedAt);

        return query;
    }
}

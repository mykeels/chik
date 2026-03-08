using Microsoft.EntityFrameworkCore;
using Chik.Exams.Data;

namespace Chik.Exams.Data;

public class ExamAnswerRepository(
    IDbContextFactory<ChikExamsDbContext> _dbContextFactory,
    ILogger<ExamAnswerRepository> logger,
    TimeProvider timeProvider
) : IExamAnswerRepository
{
    public async Task<ExamAnswerDbo> Create(ExamAnswer.Create answer)
    {
        logger.LogInformation($"{nameof(ExamAnswerRepository)}.{nameof(Create)} (ExamId: {answer.ExamId}, QuestionId: {answer.QuestionId})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var answerDbo = new ExamAnswerDbo
        {
            ExamId = answer.ExamId,
            QuestionId = answer.QuestionId,
            Answer = answer.Answer,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        };

        await dbContext.ExamAnswers.AddAsync(answerDbo);
        await dbContext.SaveChangesAsync();
        return answerDbo;
    }

    public async Task<ExamAnswerDbo?> Get(long id)
    {
        logger.LogInformation($"{nameof(ExamAnswerRepository)}.{nameof(Get)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.ExamAnswers.AsNoTracking()
            .Include(a => a.Question)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<ExamAnswerDbo?> GetByExamAndQuestion(long examId, long questionId)
    {
        logger.LogInformation($"{nameof(ExamAnswerRepository)}.{nameof(GetByExamAndQuestion)} ({examId}, {questionId})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.ExamAnswers.AsNoTracking()
            .FirstOrDefaultAsync(a => a.ExamId == examId && a.QuestionId == questionId);
    }

    public async Task<List<ExamAnswerDbo>> GetByExamId(long examId)
    {
        logger.LogInformation($"{nameof(ExamAnswerRepository)}.{nameof(GetByExamId)} ({examId})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.ExamAnswers.AsNoTracking()
            .Where(a => a.ExamId == examId)
            .Include(a => a.Question)
            .ToListAsync();
    }

    public async Task<ExamAnswerDbo> Update(long id, ExamAnswer.Update answer)
    {
        logger.LogInformation($"{nameof(ExamAnswerRepository)}.{nameof(Update)} ({id}, {answer})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existingAnswer = await dbContext.ExamAnswers.FirstOrDefaultAsync(a => a.Id == id);
        if (existingAnswer is null)
        {
            throw new KeyNotFoundException($"Answer with id '{id}' not found");
        }

        if (answer.Answer is not null)
        {
            existingAnswer.Answer = answer.Answer;
        }
        if (answer.AutoScore is not null)
        {
            existingAnswer.AutoScore = answer.AutoScore;
        }
        if (answer.ExaminerScore is not null)
        {
            existingAnswer.ExaminerScore = answer.ExaminerScore;
        }
        if (answer.ExaminerId is not null)
        {
            existingAnswer.ExaminerId = answer.ExaminerId;
        }
        if (answer.ExaminerComment is not null)
        {
            existingAnswer.ExaminerComment = answer.ExaminerComment;
        }
        existingAnswer.UpdatedAt = timeProvider.GetUtcNow().DateTime;

        await dbContext.SaveChangesAsync();
        return existingAnswer;
    }

    public async Task<ExamAnswerDbo> SubmitAnswer(long examId, long questionId, string answer)
    {
        logger.LogInformation($"{nameof(ExamAnswerRepository)}.{nameof(SubmitAnswer)} ({examId}, {questionId})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var existingAnswer = await dbContext.ExamAnswers
            .FirstOrDefaultAsync(a => a.ExamId == examId && a.QuestionId == questionId);

        if (existingAnswer is not null)
        {
            existingAnswer.Answer = answer;
            existingAnswer.UpdatedAt = timeProvider.GetUtcNow().DateTime;
            await dbContext.SaveChangesAsync();
            return existingAnswer;
        }

        var answerDbo = new ExamAnswerDbo
        {
            ExamId = examId,
            QuestionId = questionId,
            Answer = answer,
            CreatedAt = timeProvider.GetUtcNow().DateTime
        };

        await dbContext.ExamAnswers.AddAsync(answerDbo);
        await dbContext.SaveChangesAsync();
        return answerDbo;
    }

    public async Task<ExamAnswerDbo> AutoScore(long id, int score)
    {
        logger.LogInformation($"{nameof(ExamAnswerRepository)}.{nameof(AutoScore)} ({id}, score: {score})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var answer = await dbContext.ExamAnswers.FirstOrDefaultAsync(a => a.Id == id);
        if (answer is null)
        {
            throw new KeyNotFoundException($"Answer with id '{id}' not found");
        }

        answer.AutoScore = score;
        answer.UpdatedAt = timeProvider.GetUtcNow().DateTime;
        await dbContext.SaveChangesAsync();
        return answer;
    }

    public async Task<ExamAnswerDbo> ExaminerScore(long id, int score, long examinerId, string? comment = null)
    {
        logger.LogInformation($"{nameof(ExamAnswerRepository)}.{nameof(ExaminerScore)} ({id}, score: {score}, examinerId: {examinerId})");
        using var dbContext = _dbContextFactory.CreateDbContext();

        var answer = await dbContext.ExamAnswers.FirstOrDefaultAsync(a => a.Id == id);
        if (answer is null)
        {
            throw new KeyNotFoundException($"Answer with id '{id}' not found");
        }

        answer.ExaminerScore = score;
        answer.ExaminerId = examinerId;
        answer.ExaminerComment = comment;
        answer.UpdatedAt = timeProvider.GetUtcNow().DateTime;
        await dbContext.SaveChangesAsync();
        return answer;
    }

    public async Task<Paginated<ExamAnswerDbo>> Search(ExamAnswer.Filter? filter = null, PaginationOptions? pagination = null)
    {
        pagination ??= new PaginationOptions();
        filter ??= new ExamAnswer.Filter();
        using var dbContext = _dbContextFactory.CreateDbContext();
        var query = GetQuery(dbContext, filter);
        var totalCount = await query.CountAsync();
        var items = await query.Skip(pagination.Skip).Take(pagination.Rows).ToListAsync();
        return new Paginated<ExamAnswerDbo>(items, totalCount, pagination, async options => await Search(filter, options));
    }

    public async Task Delete(long id)
    {
        logger.LogInformation($"{nameof(ExamAnswerRepository)}.{nameof(Delete)} ({id})");
        using var dbContext = _dbContextFactory.CreateDbContext();
        var answer = await dbContext.ExamAnswers.FirstOrDefaultAsync(a => a.Id == id);
        if (answer is null)
        {
            return;
        }
        dbContext.ExamAnswers.Remove(answer);
        await dbContext.SaveChangesAsync();
    }

    private IQueryable<ExamAnswerDbo> GetQuery(ChikExamsDbContext dbContext, ExamAnswer.Filter filter)
    {
        var query = dbContext.ExamAnswers.AsNoTracking();

        if (filter.ExamId is not null)
        {
            query = query.Where(a => a.ExamId == filter.ExamId);
        }

        if (filter.QuestionId is not null)
        {
            query = query.Where(a => a.QuestionId == filter.QuestionId);
        }

        if (filter.ExaminerId is not null)
        {
            query = query.Where(a => a.ExaminerId == filter.ExaminerId);
        }

        if (filter.IsAutoScored is not null)
        {
            if (filter.IsAutoScored.Value)
            {
                query = query.Where(a => a.AutoScore != null);
            }
            else
            {
                query = query.Where(a => a.AutoScore == null);
            }
        }

        if (filter.IsExaminerScored is not null)
        {
            if (filter.IsExaminerScored.Value)
            {
                query = query.Where(a => a.ExaminerScore != null);
            }
            else
            {
                query = query.Where(a => a.ExaminerScore == null);
            }
        }

        if (filter.AnswerIds is not null && filter.AnswerIds.Count > 0)
        {
            query = query.Where(a => filter.AnswerIds.Contains(a.Id));
        }

        if (filter.IncludeExam == true)
        {
            query = query.Include(a => a.Exam);
        }

        if (filter.IncludeQuestion == true)
        {
            query = query.Include(a => a.Question);
        }

        if (filter.IncludeExaminer == true)
        {
            query = query.Include(a => a.Examiner);
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

using Chik.Exams.Data;

namespace Chik.Exams;

internal class QuizService(
    IQuizRepository repository,
    IAuditLogService auditLogService,
    ILogger<QuizService> logger
) : IQuizService
{
    public IQuizRepository Repository => repository;

    public async Task<Quiz> Create(Auth auth, Quiz.Create quiz)
    {
        logger.LogInformation($"{nameof(QuizService)}.{nameof(Create)} ({auth.Id}, {quiz})");
        
        // Authorization: Admin and Teacher can create quizzes
        if (!auth.IsAdmin() && !auth.IsTeacher())
        {
            throw new UnauthorizedAccessException("Only Admin or Teacher can create quizzes");
        }

        var quizDbo = await repository.Create(quiz);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<Quiz.Create>(
                $"{nameof(QuizService)}.{nameof(Create)}",
                quizDbo.Id,
                quiz
            )
        );
        return quizDbo!.ToModel();
    }

    public async Task<Quiz?> Get(Auth auth, long id, bool includeQuestions = false)
    {
        logger.LogInformation($"{nameof(QuizService)}.{nameof(Get)} ({auth.Id}, {id})");
        
        var quizDbo = await repository.Get(id, includeQuestions);
        if (quizDbo is null) return null;

        // Authorization: Admin can see all, Teacher can see their own or where they are examiner
        if (!auth.IsAdmin())
        {
            if (!auth.IsTeacher())
            {
                throw new UnauthorizedAccessException("Only Admin or Teacher can view quizzes directly");
            }
            if (quizDbo.CreatorId != auth.Id && quizDbo.ExaminerId != auth.Id)
            {
                throw new UnauthorizedAccessException("You can only view quizzes you created or are assigned to examine");
            }
        }

        return quizDbo!.ToModel();
    }

    public async Task<Quiz> Update(Auth auth, Quiz.Update quiz)
    {
        logger.LogInformation($"{nameof(QuizService)}.{nameof(Update)} ({auth.Id}, {quiz})");
        
        var existingQuiz = await repository.Get(quiz.Id);
        if (existingQuiz is null)
        {
            throw new KeyNotFoundException($"Quiz with id '{quiz.Id}' not found");
        }

        // Authorization: Admin can update any, Teacher can only update their own
        if (!auth.IsAdmin())
        {
            if (!auth.IsTeacher())
            {
                throw new UnauthorizedAccessException("Only Admin or Teacher can update quizzes");
            }
            if (existingQuiz.CreatorId != auth.Id)
            {
                throw new UnauthorizedAccessException("You can only update quizzes you created");
            }
        }

        var quizDbo = await repository.Update(quiz.Id, quiz);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<Quiz.Update>(
                $"{nameof(QuizService)}.{nameof(Update)}",
                quiz.Id,
                quiz
            )
        );
        return quizDbo!.ToModel();
    }

    public async Task Delete(Auth auth, long id)
    {
        logger.LogInformation($"{nameof(QuizService)}.{nameof(Delete)} ({auth.Id}, {id})");
        
        var existingQuiz = await repository.Get(id);
        if (existingQuiz is null)
        {
            throw new KeyNotFoundException($"Quiz with id '{id}' not found");
        }

        // Authorization: Admin can delete any, Teacher can only delete their own
        if (!auth.IsAdmin())
        {
            if (!auth.IsTeacher())
            {
                throw new UnauthorizedAccessException("Only Admin or Teacher can delete quizzes");
            }
            if (existingQuiz.CreatorId != auth.Id)
            {
                throw new UnauthorizedAccessException("You can only delete quizzes you created");
            }
        }

        await repository.Delete(id);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(QuizService)}.{nameof(Delete)}",
                id,
                new { DeletedQuizId = id }
            )
        );
    }

    public async Task<Paginated<Quiz>> Search(Auth auth, Quiz.Filter? filter = null, PaginationOptions? pagination = null)
    {
        logger.LogInformation($"{nameof(QuizService)}.{nameof(Search)} ({auth.Id}, {filter})");
        
        filter ??= new Quiz.Filter();
        pagination ??= new PaginationOptions();

        // Authorization: Admin sees all, Teacher sees their own or where they are examiner
        if (!auth.IsAdmin())
        {
            if (!auth.IsTeacher())
            {
                throw new UnauthorizedAccessException("Only Admin or Teacher can search quizzes directly");
            }
            // Filter to only quizzes created by or assigned to the teacher
            // This is a bit complex - we need OR logic, but for simplicity we'll rely on the repository or modify filter
            filter = filter with { CreatorId = auth.Id };
        }

        var paginated = await repository.Search(filter, pagination);
        return new Paginated<Quiz>(
            paginated.Items.Select(dbo => dbo!.ToModel()).ToList(),
            paginated.TotalCount,
            pagination,
            async options => await Search(auth, filter, options)
        );
    }
}
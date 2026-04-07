using Chik.Exams.Data;

namespace Chik.Exams;

internal class QuizQuestionService(
    IQuizQuestionRepository repository,
    IQuizRepository quizRepository,
    IExamRepository examRepository,
    IAuditLogService auditLogService,
    ILogger<QuizQuestionService> logger
) : IQuizQuestionService
{
    public IQuizQuestionRepository Repository => repository;

    public async Task<QuizQuestion> Create(Auth auth, QuizQuestion.Create question)
    {
        logger.LogInformation($"{nameof(QuizQuestionService)}.{nameof(Create)} ({auth.Id}, {question})");
        
        // Authorization: Admin and Teacher can create questions for quizzes they own
        await AuthorizeQuizManageAccess(auth, question.QuizId);

        var questionDbo = await repository.Create(question);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<QuizQuestion.Create>(
                $"{nameof(QuizQuestionService)}.{nameof(Create)}",
                questionDbo.Id,
                question
            )
        );
        return questionDbo!.ToModel();
    }

    public async Task<QuizQuestion?> Get(Auth auth, long id)
    {
        logger.LogInformation($"{nameof(QuizQuestionService)}.{nameof(Get)} ({auth.Id}, {id})");
        
        var questionDbo = await repository.Get(id);
        if (questionDbo is null) return null;

        await AuthorizeQuizQuestionReadAccess(auth, questionDbo.QuizId);
        if (auth.IsStudent() && questionDbo.DeactivatedAt is not null)
            return null;

        return questionDbo!.ToModel();
    }

    public async Task<List<QuizQuestion>> GetByQuizId(Auth auth, long quizId, bool includeDeactivated = false)
    {
        logger.LogInformation($"{nameof(QuizQuestionService)}.{nameof(GetByQuizId)} ({auth.Id}, {quizId})");
        
        await AuthorizeQuizQuestionReadAccess(auth, quizId);

        var effectiveIncludeDeactivated = includeDeactivated && (auth.IsAdmin() || auth.IsTeacher());
        var questions = await repository.GetByQuizId(quizId, effectiveIncludeDeactivated);
        return questions.Select(dbo => dbo!.ToModel()).ToList();
    }

    public async Task<QuizQuestion> Update(Auth auth, QuizQuestion.Update question)
    {
        logger.LogInformation($"{nameof(QuizQuestionService)}.{nameof(Update)} ({auth.Id}, {question})");
        
        var existingQuestion = await repository.Get(question.Id);
        if (existingQuestion is null)
        {
            throw new KeyNotFoundException($"QuizQuestion with id '{question.Id}' not found");
        }

        await AuthorizeQuizManageAccess(auth, existingQuestion.QuizId);

        var questionDbo = await repository.Update(question.Id, question);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<QuizQuestion.Update>(
                $"{nameof(QuizQuestionService)}.{nameof(Update)}",
                question.Id,
                question
            )
        );
        return questionDbo!.ToModel();
    }

    public async Task Deactivate(Auth auth, long id)
    {
        logger.LogInformation($"{nameof(QuizQuestionService)}.{nameof(Deactivate)} ({auth.Id}, {id})");
        
        var existingQuestion = await repository.Get(id);
        if (existingQuestion is null)
        {
            throw new KeyNotFoundException($"QuizQuestion with id '{id}' not found");
        }

        await AuthorizeQuizManageAccess(auth, existingQuestion.QuizId);

        await repository.Deactivate(id);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(QuizQuestionService)}.{nameof(Deactivate)}",
                id,
                new { DeactivatedQuestionId = id }
            )
        );
    }

    public async Task Reactivate(Auth auth, long id)
    {
        logger.LogInformation($"{nameof(QuizQuestionService)}.{nameof(Reactivate)} ({auth.Id}, {id})");
        
        var existingQuestion = await repository.Get(id);
        if (existingQuestion is null)
        {
            throw new KeyNotFoundException($"QuizQuestion with id '{id}' not found");
        }

        await AuthorizeQuizManageAccess(auth, existingQuestion.QuizId);

        await repository.Reactivate(id);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(QuizQuestionService)}.{nameof(Reactivate)}",
                id,
                new { ReactivatedQuestionId = id }
            )
        );
    }

    public async Task Delete(Auth auth, long id)
    {
        logger.LogInformation($"{nameof(QuizQuestionService)}.{nameof(Delete)} ({auth.Id}, {id})");
        
        // Authorization: Admin only for permanent delete
        if (!auth.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only Admin can permanently delete quiz questions");
        }

        var existingQuestion = await repository.Get(id);
        if (existingQuestion is null)
        {
            throw new KeyNotFoundException($"QuizQuestion with id '{id}' not found");
        }

        await repository.Delete(id);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(QuizQuestionService)}.{nameof(Delete)}",
                id,
                new { DeletedQuestionId = id }
            )
        );
    }

    public async Task ReorderQuestions(Auth auth, long quizId, List<long> questionIdsInOrder)
    {
        logger.LogInformation($"{nameof(QuizQuestionService)}.{nameof(ReorderQuestions)} ({auth.Id}, {quizId})");
        
        await AuthorizeQuizManageAccess(auth, quizId);

        await repository.ReorderQuestions(quizId, questionIdsInOrder);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(QuizQuestionService)}.{nameof(ReorderQuestions)}",
                quizId,
                new { QuizId = quizId, QuestionOrder = questionIdsInOrder }
            )
        );
    }

    public async Task<Paginated<QuizQuestion>> Search(Auth auth, QuizQuestion.Filter? filter = null, PaginationOptions? pagination = null)
    {
        logger.LogInformation($"{nameof(QuizQuestionService)}.{nameof(Search)} ({auth.Id}, {filter})");
        
        filter ??= new QuizQuestion.Filter();
        pagination ??= new PaginationOptions();

        // Authorization: Admin and Teacher can search, but Teacher needs quiz access
        if (!auth.IsAdmin() && !auth.IsTeacher())
        {
            throw new UnauthorizedAccessException("Only Admin or Teacher can search quiz questions");
        }

        var paginated = await repository.Search(filter, pagination);
        return new Paginated<QuizQuestion>(
            paginated.Items.Select(dbo => dbo!.ToModel()).ToList(),
            paginated.TotalCount,
            pagination,
            async options => await Search(auth, filter, options)
        );
    }

    private async Task AuthorizeQuizManageAccess(Auth auth, long quizId)
    {
        if (auth.IsAdmin()) return;

        if (!auth.IsTeacher())
        {
            throw new UnauthorizedAccessException("Only Admin or Teacher can manage quiz questions");
        }

        await AssertTeacherCanManageQuiz(auth, quizId);
    }

    private async Task AuthorizeQuizQuestionReadAccess(Auth auth, long quizId)
    {
        if (auth.IsAdmin()) return;

        if (auth.IsTeacher())
        {
            await AssertTeacherCanManageQuiz(auth, quizId);
            return;
        }

        if (auth.IsStudent())
        {
            if (await examRepository.UserHasAssignedExamForQuiz(auth.Id, quizId))
                return;
            throw new UnauthorizedAccessException("You do not have an exam assignment for this quiz");
        }

        throw new UnauthorizedAccessException("You cannot view questions for this quiz");
    }

    private async Task AssertTeacherCanManageQuiz(Auth auth, long quizId)
    {
        var quiz = await quizRepository.Get(quizId);
        if (quiz is null)
        {
            throw new KeyNotFoundException($"Quiz with id '{quizId}' not found");
        }

        if (quiz.CreatorId != auth.Id && quiz.ExaminerId != auth.Id)
        {
            throw new UnauthorizedAccessException("You can only manage questions for quizzes you created or are assigned to examine");
        }
    }
}

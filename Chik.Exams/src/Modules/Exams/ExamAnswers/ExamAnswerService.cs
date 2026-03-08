using Chik.Exams.Data;

namespace Chik.Exams;

internal class ExamAnswerService(
    IExamAnswerRepository repository,
    IExamRepository examRepository,
    IAuditLogService auditLogService,
    ILogger<ExamAnswerService> logger
) : IExamAnswerService
{
    public IExamAnswerRepository Repository => repository;

    public async Task<ExamAnswer> SubmitAnswer(Auth auth, long examId, long questionId, string answer)
    {
        logger.LogInformation($"{nameof(ExamAnswerService)}.{nameof(SubmitAnswer)} ({auth.Id}, {examId}, {questionId})");
        
        var exam = await examRepository.Get(examId);
        if (exam is null)
        {
            throw new KeyNotFoundException($"Exam with id '{examId}' not found");
        }

        // Authorization: Only the student taking the exam can submit
        if (exam.UserId != auth.Id)
        {
            throw new UnauthorizedAccessException("Only the assigned student can submit answers for this exam");
        }

        if (exam.StartedAt is null)
        {
            throw new InvalidOperationException("Exam has not been started yet");
        }

        if (exam.EndedAt is not null)
        {
            throw new InvalidOperationException("Exam has already been submitted");
        }

        var answerDbo = await repository.SubmitAnswer(examId, questionId, answer);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(ExamAnswerService)}.{nameof(SubmitAnswer)}",
                answerDbo.Id,
                new { ExamId = examId, QuestionId = questionId }
            )
        );
        return answerDbo!.ToModel();
    }

    public async Task<ExamAnswer?> Get(Auth auth, long id)
    {
        logger.LogInformation($"{nameof(ExamAnswerService)}.{nameof(Get)} ({auth.Id}, {id})");
        
        var answerDbo = await repository.Get(id);
        if (answerDbo is null) return null;

        await AuthorizeExamAccess(auth, answerDbo.ExamId);
        return answerDbo!.ToModel();
    }

    public async Task<List<ExamAnswer>> GetByExamId(Auth auth, long examId)
    {
        logger.LogInformation($"{nameof(ExamAnswerService)}.{nameof(GetByExamId)} ({auth.Id}, {examId})");
        
        await AuthorizeExamAccess(auth, examId);

        var answers = await repository.GetByExamId(examId);
        return answers.Select(dbo => dbo!.ToModel()).ToList();
    }

    public async Task<ExamAnswer> Update(Auth auth, ExamAnswer.Update answer)
    {
        logger.LogInformation($"{nameof(ExamAnswerService)}.{nameof(Update)} ({auth.Id}, {answer})");
        
        var existingAnswer = await repository.Get(answer.Id);
        if (existingAnswer is null)
        {
            throw new KeyNotFoundException($"ExamAnswer with id '{answer.Id}' not found");
        }

        var exam = await examRepository.Get(existingAnswer.ExamId);
        if (exam is null)
        {
            throw new KeyNotFoundException($"Exam with id '{existingAnswer.ExamId}' not found");
        }

        // Authorization: Only the student taking the exam can update before submission
        if (exam.UserId != auth.Id)
        {
            throw new UnauthorizedAccessException("Only the assigned student can update answers for this exam");
        }

        if (exam.EndedAt is not null)
        {
            throw new InvalidOperationException("Cannot update answers after exam submission");
        }

        var answerDbo = await repository.Update(answer.Id, answer);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<ExamAnswer.Update>(
                $"{nameof(ExamAnswerService)}.{nameof(Update)}",
                answer.Id,
                answer
            )
        );
        return answerDbo!.ToModel();
    }

    public async Task<ExamAnswer> ExaminerScore(Auth auth, long answerId, int score, string? comment = null)
    {
        logger.LogInformation($"{nameof(ExamAnswerService)}.{nameof(ExaminerScore)} ({auth.Id}, {answerId}, {score})");
        
        var existingAnswer = await repository.Get(answerId);
        if (existingAnswer is null)
        {
            throw new KeyNotFoundException($"ExamAnswer with id '{answerId}' not found");
        }

        var exam = await examRepository.Get(existingAnswer.ExamId);
        if (exam is null)
        {
            throw new KeyNotFoundException($"Exam with id '{existingAnswer.ExamId}' not found");
        }

        // Authorization: Admin and Teacher (creator/examiner) can score
        if (!auth.IsAdmin())
        {
            if (!auth.IsTeacher())
            {
                throw new UnauthorizedAccessException("Only Admin or Teacher can score answers");
            }
            if (exam.CreatorId != auth.Id && exam.ExaminerId != auth.Id)
            {
                throw new UnauthorizedAccessException("You can only score answers for exams you created or are assigned to examine");
            }
        }

        if (exam.EndedAt is null)
        {
            throw new InvalidOperationException("Cannot score answers for an exam that has not been submitted");
        }

        var answerDbo = await repository.ExaminerScore(answerId, score, auth.Id, comment);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(ExamAnswerService)}.{nameof(ExaminerScore)}",
                answerId,
                new { AnswerId = answerId, Score = score, Comment = comment }
            )
        );
        return answerDbo!.ToModel();
    }

    public async Task<Paginated<ExamAnswer>> Search(Auth auth, ExamAnswer.Filter? filter = null, PaginationOptions? pagination = null)
    {
        logger.LogInformation($"{nameof(ExamAnswerService)}.{nameof(Search)} ({auth.Id}, {filter})");
        
        filter ??= new ExamAnswer.Filter();
        pagination ??= new PaginationOptions();

        // Authorization: Admin and Teacher can search all, Student can only search their own
        if (!auth.IsAdmin() && !auth.IsTeacher())
        {
            // For students, we need to filter by exams they took
            // This is a limitation - ideally we'd have ExamUserId in the filter
            throw new UnauthorizedAccessException("Students should use GetByExamId instead of Search");
        }

        var paginated = await repository.Search(filter, pagination);
        return new Paginated<ExamAnswer>(
            paginated.Items.Select(dbo => dbo!.ToModel()).ToList(),
            paginated.TotalCount,
            pagination,
            async options => await Search(auth, filter, options)
        );
    }

    private async Task AuthorizeExamAccess(Auth auth, long examId)
    {
        var exam = await examRepository.Get(examId);
        if (exam is null)
        {
            throw new KeyNotFoundException($"Exam with id '{examId}' not found");
        }

        if (auth.IsAdmin()) return;

        if (auth.IsTeacher())
        {
            if (exam.CreatorId != auth.Id && exam.ExaminerId != auth.Id)
            {
                throw new UnauthorizedAccessException("You can only view answers for exams you created or are assigned to examine");
            }
        }
        else if (auth.IsStudent())
        {
            if (exam.UserId != auth.Id)
            {
                throw new UnauthorizedAccessException("You can only view your own exam answers");
            }
        }
        else
        {
            throw new UnauthorizedAccessException("Unauthorized to view exam answers");
        }
    }
}

using Chik.Exams.Data;

namespace Chik.Exams;

internal class ExamService(
    IExamRepository repository,
    IExamAnswerRepository answerRepository,
    IQuizRepository quizRepository,
    IQuizQuestionRepository questionRepository,
    IUserRepository userRepository,
    IClassRepository classRepository,
    IAuditLogService auditLogService,
    ILogger<ExamService> logger
) : IExamService
{
    public IExamRepository Repository => repository;

    public async Task<Exam> Create(Auth auth, Exam.Create exam)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(Create)} ({auth.Id}, {exam})");
        
        // Authorization: Admin and Teacher can create exams
        if (!auth.IsAdmin() && !auth.IsTeacher())
        {
            throw new UnauthorizedAccessException("Only Admin or Teacher can create exams");
        }

        var targetUser = await userRepository.Get(exam.UserId);
        if (targetUser is null)
            throw new KeyNotFoundException($"User with id '{exam.UserId}' was not found");
        if ((targetUser.Roles & (int)UserRole.Student) == 0)
            throw new InvalidOperationException("Exams can only be assigned to users with the Student role");
        var studentClassId = await userRepository.GetStudentClassIdForUser(exam.UserId);
        if (studentClassId is null)
            throw new InvalidOperationException("Student must belong to a class before an exam can be assigned");

        if (auth.IsTeacher() && !auth.IsAdmin())
        {
            var teacherClasses = await classRepository.GetClassIdsForTeacher(auth.Id);
            if (!teacherClasses.Contains(studentClassId.Value))
                throw new UnauthorizedAccessException("You can only assign exams to students in classes you teach");
        }

        var create = exam with { StudentClassId = studentClassId.Value };
        var examDbo = await repository.Create(create);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<Exam.Create>(
                $"{nameof(ExamService)}.{nameof(Create)}",
                examDbo.Id,
                create
            )
        );
        return examDbo!.ToModel();
    }

    public async Task<List<Exam>> AssignToClass(Auth auth, int classId, long quizId)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(AssignToClass)} ({auth.Id}, class {classId}, quiz {quizId})");

        if (!auth.IsAdmin() && !auth.IsTeacher())
            throw new UnauthorizedAccessException("Only Admin or Teacher can assign exams");

        if (await classRepository.Get(classId) is null)
            throw new KeyNotFoundException($"Class with id '{classId}' was not found");

        if (auth.IsTeacher() && !auth.IsAdmin())
        {
            var teacherClasses = await classRepository.GetClassIdsForTeacher(auth.Id);
            if (!teacherClasses.Contains(classId))
                throw new UnauthorizedAccessException("You can only assign exams to classes you teach");
        }

        var studentIds = await userRepository.GetStudentUserIdsInClass(classId);
        var results = new List<Exam>();
        foreach (var studentId in studentIds)
        {
            if (await repository.UserHasAssignedExamForQuiz(studentId, quizId))
                continue;

            var examDbo = await repository.Create(new Exam.Create(studentId, quizId, auth.Id, classId));
            await auditLogService.Create(
                auth,
                new AuditLog.Create<Exam.Create>(
                    $"{nameof(ExamService)}.{nameof(AssignToClass)}",
                    examDbo.Id,
                    new Exam.Create(studentId, quizId, auth.Id, classId)
                )
            );
            results.Add(examDbo.ToModel());
        }

        return results;
    }

    public async Task<Exam?> Get(Auth auth, long id, bool includeAnswers = false)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(Get)} ({auth.Id}, {id})");
        
        var examDbo = await repository.Get(id, includeAnswers);
        if (examDbo is null) return null;

        // Authorization: Admin sees all, Teacher sees their created exams, Student sees their own
        if (!auth.IsAdmin())
        {
            if (auth.IsTeacher())
            {
                if (examDbo.CreatorId != auth.Id && examDbo.ExaminerId != auth.Id)
                {
                    throw new UnauthorizedAccessException("You can only view exams you created or are assigned to examine");
                }
            }
            else if (auth.IsStudent())
            {
                if (examDbo.UserId != auth.Id)
                {
                    throw new UnauthorizedAccessException("You can only view your own exams");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("Unauthorized to view exams");
            }
        }

        return examDbo!.ToModel();
    }

    public async Task<Exam> Update(Auth auth, Exam.Update exam)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(Update)} ({auth.Id}, {exam})");
        
        var existingExam = await repository.Get(exam.Id);
        if (existingExam is null)
        {
            throw new KeyNotFoundException($"Exam with id '{exam.Id}' not found");
        }

        // Authorization: Admin can update any, Teacher can update their created exams
        if (!auth.IsAdmin())
        {
            if (!auth.IsTeacher())
            {
                throw new UnauthorizedAccessException("Only Admin or Teacher can update exams");
            }
            if (existingExam.CreatorId != auth.Id)
            {
                throw new UnauthorizedAccessException("You can only update exams you created");
            }
        }

        var examDbo = await repository.Update(exam.Id, exam);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<Exam.Update>(
                $"{nameof(ExamService)}.{nameof(Update)}",
                exam.Id,
                exam
            )
        );
        return examDbo!.ToModel();
    }

    public async Task<Exam> Start(Auth auth, long id)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(Start)} ({auth.Id}, {id})");
        
        var examDbo = await repository.Get(id);
        if (examDbo is null)
        {
            throw new KeyNotFoundException($"Exam with id '{id}' not found");
        }

        // Authorization: Only the assigned student can start
        if (examDbo.UserId != auth.Id)
        {
            throw new UnauthorizedAccessException("Only the assigned student can start this exam");
        }

        if (examDbo.StartedAt is not null)
        {
            throw new InvalidOperationException("Exam has already been started");
        }

        var result = await repository.Start(id);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(ExamService)}.{nameof(Start)}",
                id,
                new { StartedExamId = id }
            )
        );
        return result!.ToModel();
    }

    public async Task<Exam> End(Auth auth, long id)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(End)} ({auth.Id}, {id})");
        
        var examDbo = await repository.Get(id);
        if (examDbo is null)
        {
            throw new KeyNotFoundException($"Exam with id '{id}' not found");
        }

        // Authorization: Only the assigned student can end
        if (examDbo.UserId != auth.Id)
        {
            throw new UnauthorizedAccessException("Only the assigned student can submit this exam");
        }

        if (examDbo.StartedAt is null)
        {
            throw new InvalidOperationException("Exam has not been started yet");
        }

        if (examDbo.EndedAt is not null)
        {
            throw new InvalidOperationException("Exam has already been submitted");
        }

        var result = await repository.End(id);
        try
        {
            var examWithAnswers = await repository.Get(id, includeAnswers: true);
            if (examWithAnswers is not null)
                await ApplyAutoScores(examWithAnswers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Auto-score after submit failed for exam {ExamId}", id);
        }

        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(ExamService)}.{nameof(End)}",
                id,
                new { EndedExamId = id }
            )
        );
        return result!.ToModel();
    }

    public async Task<Exam> Mark(Auth auth, long id, int score, string? comment = null)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(Mark)} ({auth.Id}, {id}, {score})");
        
        var examDbo = await repository.Get(id);
        if (examDbo is null)
        {
            throw new KeyNotFoundException($"Exam with id '{id}' not found");
        }

        // Authorization: Admin and Teacher (creator/examiner) can mark
        if (!auth.IsAdmin())
        {
            if (!auth.IsTeacher())
            {
                throw new UnauthorizedAccessException("Only Admin or Teacher can mark exams");
            }
            if (examDbo.CreatorId != auth.Id && examDbo.ExaminerId != auth.Id)
            {
                throw new UnauthorizedAccessException("You can only mark exams you created or are assigned to examine");
            }
        }

        if (examDbo.EndedAt is null)
        {
            throw new InvalidOperationException("Cannot mark an exam that has not been submitted");
        }

        var result = await repository.Mark(id, score, auth.Id, comment);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(ExamService)}.{nameof(Mark)}",
                id,
                new { MarkedExamId = id, Score = score, Comment = comment }
            )
        );
        return result!.ToModel();
    }

    public async Task AutoScore(Auth auth, long examId)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(AutoScore)} ({auth.Id}, {examId})");
        
        var examDbo = await repository.Get(examId, includeAnswers: true);
        if (examDbo is null)
        {
            throw new KeyNotFoundException($"Exam with id '{examId}' not found");
        }

        // Authorization: Admin and Teacher can auto-score
        if (!auth.IsAdmin() && !auth.IsTeacher())
        {
            throw new UnauthorizedAccessException("Only Admin or Teacher can auto-score exams");
        }

        var quiz = await quizRepository.Get(examDbo.QuizId, includeQuestions: true);
        if (quiz is null || quiz.Questions is null)
        {
            throw new InvalidOperationException("Quiz or questions not found");
        }

        await ApplyAutoScores(examDbo, quiz);

        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(ExamService)}.{nameof(AutoScore)}",
                examId,
                new { AutoScoredExamId = examId }
            )
        );
    }

    /// <summary>
    /// Computes and persists auto-scores for each answer. Used after submit and from the teacher auto-score endpoint.
    /// </summary>
    private async Task ApplyAutoScores(ExamDbo examDbo, QuizDbo? quiz = null)
    {
        quiz ??= await quizRepository.Get(examDbo.QuizId, includeQuestions: true);
        if (quiz?.Questions is null)
            return;

        var answers = examDbo.Answers ?? [];
        foreach (var answer in answers)
        {
            var question = quiz.Questions!.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question is null) continue;

            var autoScore = CalculateAutoScore(question, answer.Answer);
            if (autoScore is not null)
            {
                await answerRepository.AutoScore(answer.Id, autoScore.Value);
            }
        }
    }

    public async Task<ExamScores> GetScores(Auth auth, long examId)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(GetScores)} ({auth.Id}, {examId})");
        
        var examDbo = await repository.Get(examId, includeAnswers: true);
        if (examDbo is null)
        {
            throw new KeyNotFoundException($"Exam with id '{examId}' not found");
        }

        // Authorization: Admin, Teacher (creator/examiner), or the student
        if (!auth.IsAdmin())
        {
            if (auth.IsTeacher())
            {
                if (examDbo.CreatorId != auth.Id && examDbo.ExaminerId != auth.Id)
                {
                    throw new UnauthorizedAccessException("You can only view scores for exams you created or are assigned to examine");
                }
            }
            else if (auth.IsStudent())
            {
                if (examDbo.UserId != auth.Id)
                {
                    throw new UnauthorizedAccessException("You can only view your own exam scores");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("Unauthorized to view exam scores");
            }
        }

        var questions = await questionRepository.GetByQuizId(examDbo.QuizId);
        var answers = examDbo.Answers ?? [];

        var answerScores = questions.Select(q =>
        {
            var answer = answers.FirstOrDefault(a => a.QuestionId == q.Id);
            var autoScore = answer?.AutoScore;
            var examinerScore = answer?.ExaminerScore;
            // Examiner score overrides auto score
            var finalScore = examinerScore ?? autoScore ?? 0;
            return new AnswerScore(q.Id, autoScore, examinerScore, finalScore, q.Score);
        }).ToList();

        var totalScore = answerScores.Sum(a => a.FinalScore);
        var maxPossibleScore = questions.Sum(q => q.Score);
        var answeredQuestions = answers.Count;
        var totalQuestions = questions.Count;

        return new ExamScores(examId, totalScore, maxPossibleScore, answeredQuestions, totalQuestions, answerScores);
    }

    public async Task Cancel(Auth auth, long id)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(Cancel)} ({auth.Id}, {id})");
        
        var examDbo = await repository.Get(id);
        if (examDbo is null)
        {
            throw new KeyNotFoundException($"Exam with id '{id}' not found");
        }

        // Authorization: Admin can cancel any, Teacher can cancel their created exams
        if (!auth.IsAdmin())
        {
            if (!auth.IsTeacher())
            {
                throw new UnauthorizedAccessException("Only Admin or Teacher can cancel exams");
            }
            if (examDbo.CreatorId != auth.Id)
            {
                throw new UnauthorizedAccessException("You can only cancel exams you created");
            }
        }

        await repository.Delete(id);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(ExamService)}.{nameof(Cancel)}",
                id,
                new { CancelledExamId = id }
            )
        );
    }

    public async Task Delete(Auth auth, long id)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(Delete)} ({auth.Id}, {id})");
        
        // Authorization: Admin only
        if (!auth.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only Admin can delete exams");
        }

        await repository.Delete(id);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(ExamService)}.{nameof(Delete)}",
                id,
                new { DeletedExamId = id }
            )
        );
    }

    public async Task<List<Exam>> GetPendingExams(Auth auth, long? studentId = null)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(GetPendingExams)} ({auth.Id}, {studentId})");
        
        var targetStudentId = studentId ?? auth.Id;

        // Authorization: Admin/Teacher can view any student, Student can only view their own
        if (!auth.IsAdmin() && !auth.IsTeacher())
        {
            if (targetStudentId != auth.Id)
            {
                throw new UnauthorizedAccessException("You can only view your own pending exams");
            }
        }

        var filter = new Exam.Filter(
            UserId: targetStudentId,
            IsStarted: false,
            IncludeQuiz: true,
            IncludeStudentClass: true
        );
        var paginated = await repository.Search(filter, new PaginationOptions(1, 100));
        return paginated.Items.Select(dbo => dbo!.ToModel()).ToList();
    }

    public async Task<List<Exam>> GetExamHistory(Auth auth, long? studentId = null)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(GetExamHistory)} ({auth.Id}, {studentId})");
        
        var targetStudentId = studentId ?? auth.Id;

        // Authorization: Admin/Teacher can view any student, Student can only view their own
        if (!auth.IsAdmin() && !auth.IsTeacher())
        {
            if (targetStudentId != auth.Id)
            {
                throw new UnauthorizedAccessException("You can only view your own exam history");
            }
        }

        var filter = new Exam.Filter(
            UserId: targetStudentId,
            IsEnded: true,
            IncludeQuiz: true,
            IncludeStudentClass: true
        );
        var paginated = await repository.Search(filter, new PaginationOptions(1, 100));
        return paginated.Items.Select(dbo => dbo!.ToModel()).ToList();
    }

    public async Task<Paginated<Exam>> Search(Auth auth, Exam.Filter? filter = null, PaginationOptions? pagination = null)
    {
        logger.LogInformation($"{nameof(ExamService)}.{nameof(Search)} ({auth.Id}, {filter})");
        
        filter ??= new Exam.Filter();
        pagination ??= new PaginationOptions();
        filter = filter with { IncludeStudentClass = true };

        // Authorization: Admin sees all, Teacher sees their created/examined exams, Student sees their own
        if (!auth.IsAdmin())
        {
            if (auth.IsTeacher())
            {
                filter = filter with { CreatorId = auth.Id };
            }
            else if (auth.IsStudent())
            {
                filter = filter with { UserId = auth.Id };
            }
            else
            {
                throw new UnauthorizedAccessException("Unauthorized to search exams");
            }
        }

        var paginated = await repository.Search(filter, pagination);
        return new Paginated<Exam>(
                paginated.Items.Select(dbo => dbo!.ToModel()).ToList(),
            paginated.TotalCount,
            pagination,
            async options => await Search(auth, filter, options)
        );
    }

    private int? CalculateAutoScore(QuizQuestionDbo question, string answer)
    {
        if (string.IsNullOrEmpty(question.Properties)) return null;

        var properties = QuizQuestionDbo.DeserializeProperties(question.Properties);
        if (properties is null) return null;

        return properties switch
        {
            QuizQuestion.SingleChoice singleChoice =>
                singleChoice.Options.FirstOrDefault(o => o.IsCorrect)?.Text?.Equals(answer, StringComparison.OrdinalIgnoreCase) == true
                    ? question.Score : 0,

            QuizQuestion.MultipleChoice multipleChoice =>
                CalculateMultipleChoiceScore(multipleChoice, answer, question.Score),

            QuizQuestion.TrueOrFalse trueOrFalse =>
                trueOrFalse.CorrectAnswer.ToString().Equals(answer, StringComparison.OrdinalIgnoreCase)
                    ? question.Score : 0,

            QuizQuestion.FillInTheBlank fillInTheBlank =>
                fillInTheBlank.AcceptedAnswers.Any(a => a.Equals(answer, StringComparison.OrdinalIgnoreCase))
                    ? question.Score : 0,

            QuizQuestion.ShortAnswer shortAnswer =>
                shortAnswer.AcceptedAnswers?.Any(a => a.Equals(answer, StringComparison.OrdinalIgnoreCase)) == true
                    ? question.Score : 0,

            // Essay questions cannot be auto-scored
            QuizQuestion.Essay => null,

            _ => null
        };
    }

    private int CalculateMultipleChoiceScore(QuizQuestion.MultipleChoice multipleChoice, string answer, int maxScore)
    {
        try
        {
            var selectedOptions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(answer) ?? [];
            var correctOptions = multipleChoice.Options.Where(o => o.IsCorrect).Select(o => o.Text).ToList();

            var correctCount = selectedOptions.Count(s => correctOptions.Contains(s, StringComparer.OrdinalIgnoreCase));
            var incorrectCount = selectedOptions.Count(s => !correctOptions.Contains(s, StringComparer.OrdinalIgnoreCase));

            // Simple scoring: full points if all correct and no incorrect
            if (correctCount == correctOptions.Count && incorrectCount == 0)
            {
                return maxScore;
            }
            return 0;
        }
        catch
        {
            return 0;
        }
    }
}
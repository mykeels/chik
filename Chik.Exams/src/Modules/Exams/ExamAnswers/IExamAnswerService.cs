using Chik.Exams.Data;

namespace Chik.Exams;

public interface IExamAnswerService
{
    public IExamAnswerRepository Repository { get; }

    /// <summary>
    /// Submits an answer for a question in an exam. Only the student taking the exam can submit.
    /// </summary>
    Task<ExamAnswer> SubmitAnswer(Auth auth, long examId, long questionId, string answer);

    /// <summary>
    /// Gets an answer by ID.
    /// </summary>
    Task<ExamAnswer?> Get(Auth auth, long id);

    /// <summary>
    /// Gets all answers for an exam.
    /// </summary>
    Task<List<ExamAnswer>> GetByExamId(Auth auth, long examId);

    /// <summary>
    /// Updates an answer. Only the student taking the exam can update before submission.
    /// </summary>
    Task<ExamAnswer> Update(Auth auth, ExamAnswer.Update answer);

    /// <summary>
    /// Scores an answer manually (examiner score). Admin and Teacher can score.
    /// </summary>
    Task<ExamAnswer> ExaminerScore(Auth auth, long answerId, int score, string? comment = null);

    /// <summary>
    /// Searches for exam answers.
    /// </summary>
    Task<Paginated<ExamAnswer>> Search(Auth auth, ExamAnswer.Filter? filter = null, PaginationOptions? pagination = null);
}

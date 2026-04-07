namespace Chik.Exams.Data;

public interface IExamRepository
{
    Task<ExamDbo> Create(Exam.Create exam);
    Task<ExamDbo?> Get(long id, bool includeAnswers = false);
    Task<bool> UserHasAssignedExamForQuiz(long userId, long quizId);
    Task<List<ExamDbo>> GetByUserId(long userId);
    Task<List<ExamDbo>> GetByQuizId(long quizId);
    Task<ExamDbo> Update(long id, Exam.Update exam);
    Task<Paginated<ExamDbo>> Search(Exam.Filter? filter = null, PaginationOptions? pagination = null);
    Task<ExamDbo> Start(long id);
    Task<ExamDbo> End(long id);
    Task<ExamDbo> Mark(long id, int score, long examinerId, string? comment = null);
    Task Delete(long id);
}

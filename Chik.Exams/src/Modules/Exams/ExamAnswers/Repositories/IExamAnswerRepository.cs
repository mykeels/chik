namespace Chik.Exams.Data;

public interface IExamAnswerRepository
{
    Task<ExamAnswerDbo> Create(ExamAnswer.Create answer);
    Task<ExamAnswerDbo?> Get(long id);
    Task<ExamAnswerDbo?> GetByExamAndQuestion(long examId, long questionId);
    Task<List<ExamAnswerDbo>> GetByExamId(long examId);
    Task<ExamAnswerDbo> Update(long id, ExamAnswer.Update answer);
    Task<ExamAnswerDbo> SubmitAnswer(long examId, long questionId, string answer);
    Task<ExamAnswerDbo> AutoScore(long id, int score);
    Task<ExamAnswerDbo> ExaminerScore(long id, int score, long examinerId, string? comment = null);
    Task<Paginated<ExamAnswerDbo>> Search(ExamAnswer.Filter? filter = null, PaginationOptions? pagination = null);
    Task Delete(long id);
}

namespace Chik.Exams.Data;

public interface IQuizQuestionRepository
{
    Task<QuizQuestionDbo> Create(QuizQuestion.Create question);
    Task<QuizQuestionDbo?> Get(long id);
    Task<List<QuizQuestionDbo>> GetByQuizId(long quizId, bool includeDeactivated = false);
    Task<QuizQuestionDbo> Update(long id, QuizQuestion.Update question);
    Task<Paginated<QuizQuestionDbo>> Search(QuizQuestion.Filter? filter = null, PaginationOptions? pagination = null);
    Task Deactivate(long id);
    Task Reactivate(long id);
    Task Delete(long id);
    Task ReorderQuestions(long quizId, List<long> questionIdsInOrder);
}

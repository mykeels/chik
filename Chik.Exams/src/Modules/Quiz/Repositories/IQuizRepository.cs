namespace Chik.Exams.Data;

public interface IQuizRepository
{
    Task<QuizDbo> Create(Quiz.Create quiz);
    Task<QuizDbo?> Get(long id, bool includeQuestions = false);
    Task<QuizDbo> Update(long id, Quiz.Update quiz);
    Task<Paginated<QuizDbo>> Search(Quiz.Filter? filter = null, PaginationOptions? pagination = null);
    Task Delete(long id);
}

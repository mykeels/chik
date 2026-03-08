namespace Chik.Exams.Data;

public interface IQuizQuestionTypeRepository
{
    Task<QuizQuestionTypeDbo> Create(QuizQuestionType.Create type);
    Task<QuizQuestionTypeDbo?> Get(long id);
    Task<QuizQuestionTypeDbo?> GetByName(string name);
    Task<List<QuizQuestionTypeDbo>> GetAll();
    Task<QuizQuestionTypeDbo> Update(long id, QuizQuestionType.Update type);
    Task<Paginated<QuizQuestionTypeDbo>> Search(QuizQuestionType.Filter? filter = null, PaginationOptions? pagination = null);
    Task Delete(long id);
}

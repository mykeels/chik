using Chik.Exams.Data;

namespace Chik.Exams;

public interface IQuizQuestionService
{
    public IQuizQuestionRepository Repository { get; }

    /// <summary>
    /// Creates a new quiz question. Admin and Teacher can create questions for quizzes they own.
    /// </summary>
    Task<QuizQuestion> Create(Auth auth, QuizQuestion.Create question);

    /// <summary>
    /// Gets a quiz question by ID. Admin, Teacher (quiz creator/examiner), or Student with an exam for that quiz.
    /// </summary>
    Task<QuizQuestion?> Get(Auth auth, long id);

    /// <summary>
    /// Gets all questions for a quiz. Same readers as <see cref="Get"/>; students never receive deactivated questions.
    /// Admin and Teacher: when <paramref name="includeDeactivated"/> is true, the list includes deactivated items (<see cref="QuizQuestion.IsActive"/> false).
    /// </summary>
    Task<List<QuizQuestion>> GetByQuizId(Auth auth, long quizId, bool includeDeactivated = false);

    /// <summary>
    /// Updates a quiz question. Admin and Teacher can update questions for quizzes they own.
    /// </summary>
    Task<QuizQuestion> Update(Auth auth, QuizQuestion.Update question);

    /// <summary>
    /// Deactivates a quiz question (soft delete).
    /// </summary>
    Task Deactivate(Auth auth, long id);

    /// <summary>
    /// Reactivates a previously deactivated quiz question.
    /// </summary>
    Task Reactivate(Auth auth, long id);

    /// <summary>
    /// Permanently deletes a quiz question. Admin only.
    /// </summary>
    Task Delete(Auth auth, long id);

    /// <summary>
    /// Reorders questions within a quiz.
    /// </summary>
    Task ReorderQuestions(Auth auth, long quizId, List<long> questionIdsInOrder);

    /// <summary>
    /// Searches for quiz questions.
    /// </summary>
    Task<Paginated<QuizQuestion>> Search(Auth auth, QuizQuestion.Filter? filter = null, PaginationOptions? pagination = null);
}

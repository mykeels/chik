using Chik.Exams.Data;

namespace Chik.Exams;

public interface IQuizService
{
    public IQuizRepository Repository { get; }

    /// <summary>
    /// Creates a new quiz. Admin and Teacher can create quizzes.
    /// </summary>
    Task<Quiz> Create(Auth auth, Quiz.Create quiz);

    /// <summary>
    /// Gets a quiz by ID. Admin can see all, Teacher can see their own or where they are examiner.
    /// </summary>
    Task<Quiz?> Get(Auth auth, long id, bool includeQuestions = false);

    /// <summary>
    /// Updates a quiz. Admin can update any, Teacher can only update their own.
    /// </summary>
    Task<Quiz> Update(Auth auth, Quiz.Update quiz);

    /// <summary>
    /// Deletes a quiz. Admin can delete any, Teacher can only delete their own.
    /// </summary>
    Task Delete(Auth auth, long id);

    /// <summary>
    /// Searches for quizzes. Admin sees all, Teacher sees their own or where they are examiner.
    /// </summary>
    Task<Paginated<Quiz>> Search(Auth auth, Quiz.Filter? filter = null, PaginationOptions? pagination = null);
}
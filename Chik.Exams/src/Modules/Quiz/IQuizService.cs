using Chik.Exams.Data;

namespace Chik.Exams;

public interface IQuizService
{
    public IQuizRepository Repository { get; }

    Task<QuizDbo> Create(Auth auth, Quiz.Create quiz);
}
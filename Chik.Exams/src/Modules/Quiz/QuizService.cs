using Chik.Exams.Data;

namespace Chik.Exams;

internal class QuizService(
    IQuizRepository repository,
    ILogger<QuizService> logger
) : IQuizService
{
    public IQuizRepository Repository => repository;

    public async Task<QuizDbo> Create(Auth auth, Quiz.Create quiz)
    {
        logger.LogInformation($"{nameof(QuizService)}.{nameof(Create)} ({auth.Id}, {quiz})");   
        return await repository.Create(quiz);
    }
}
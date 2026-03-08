using Chik.Exams.Data;

namespace Chik.Exams;

public static class QuizExtensions
{
    public static IServiceCollection AddQuiz(this IServiceCollection services)
    {
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IQuizService, QuizService>();
        return services;
    }
}
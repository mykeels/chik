using Chik.Exams.Data;

namespace Chik.Exams;

public static class QuizExtensions
{
    public static IServiceCollection AddQuiz(this IServiceCollection services)
    {
        services.TrackScoped<IQuizRepository, QuizRepository>();
        services.TrackScoped<IQuizService, QuizService>();
        services.TrackScoped<IQuizQuestionRepository, QuizQuestionRepository>();
        services.TrackScoped<IQuizQuestionService, QuizQuestionService>();
        services.TrackScoped<IQuizQuestionTypeRepository, QuizQuestionTypeRepository>();
        services.TrackScoped<IQuizImportExportService, QuizImportExportService>();
        return services;
    }
}
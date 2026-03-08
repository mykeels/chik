using Chik.Exams.Data;

namespace Chik.Exams;

public static class ExamExtensions
{
    public static IServiceCollection AddExam(this IServiceCollection services)
    {
        services.TrackScoped<IExamAnswerRepository, ExamAnswerRepository>();
        services.TrackScoped<IExamAnswerService, ExamAnswerService>();
        services.TrackScoped<IExamRepository, ExamRepository>();
        services.TrackScoped<IExamService, ExamService>();
        return services;
    }
}
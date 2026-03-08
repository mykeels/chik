using Chik.Exams.Data;

namespace Chik.Exams;

public static class ExamExtensions
{
    public static IServiceCollection AddExam(this IServiceCollection services)
    {
        services.AddScoped<IExamRepository, ExamRepository>();
        services.AddScoped<IExamService, ExamService>();
        return services;
    }
}
namespace Chik.Exams;

public static class ChikExamsExtensions
{
    public static IServiceCollection AddChikExams(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuditLog();
        services.AddExam();
        services.AddIpAddressLocation();
        services.AddLogin();
        services.AddQuiz();
        services.AddUser();
        return services;
    }
}
namespace Chik.Exams;

public static class ChikExamsExtensions
{
    public static IServiceCollection AddChikExams(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEmailService(
            new(
                configuration["EmailCredentials:Password"] ?? throw new Exception("EmailCredentials:Password is not set")
            )
        );
        services.AddAuditLog();
        services.AddExam();
        services.AddIpAddressLocation();
        services.AddLogin();
        services.AddQuiz();
        services.AddUser();
        services.AddServerError();
        return services;
    }
}
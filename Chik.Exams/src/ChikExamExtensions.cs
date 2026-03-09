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
        services.AddJwtService(
            new JwtConfig()
            {
                Secret = configuration["Jwt:Secret"] ?? throw new Exception("Jwt:Secret is not set"),
                Issuer = configuration["Jwt:Issuer"] ?? "chik.ng",
                Audience = configuration["Jwt:Audience"] ?? "chik.ng",
                TokenExpiration = TimeSpan.FromHours(2)
            }
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
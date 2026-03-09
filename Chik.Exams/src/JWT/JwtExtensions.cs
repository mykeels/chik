namespace Chik.Exams;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtService(this IServiceCollection services, JwtConfig config)
    {
        services.AddSingleton(config);
        services.AddSingleton<IJwtService, JwtService>();
        return services;
    }
}
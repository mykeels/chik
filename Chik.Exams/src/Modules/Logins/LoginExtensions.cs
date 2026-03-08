using Microsoft.Extensions.DependencyInjection;
using Chik.Exams.Logins.Repositories;

namespace Chik.Exams;

public static class LoginExtensions
{
    public static IServiceCollection AddLogin(this IServiceCollection services)
    {
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<ILoginRepository, LoginRepository>();
        return services;
    }
}
using Chik.Exams.Data;

namespace Chik.Exams;

public static partial class UserExtensions
{
    public static IServiceCollection AddUser(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
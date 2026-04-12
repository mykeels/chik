using Chik.Exams.Data;

namespace Chik.Exams;

public static class ClassExtensions
{
    public static IServiceCollection AddClass(this IServiceCollection services)
    {
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IClassService, ClassService>();
        return services;
    }
}

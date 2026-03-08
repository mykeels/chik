using Microsoft.Extensions.DependencyInjection;
using Chik.Exams.Data;

namespace Chik.Exams;

public static class ServerErrorExtensions
{
    public static IServiceCollection AddServerError(this IServiceCollection services)
    {
        services.AddScoped<IServerErrorRepository, ServerErrorRepository>();
        services.AddScoped<IServerErrorService, ServerErrorService>();
        return services;
    }
}
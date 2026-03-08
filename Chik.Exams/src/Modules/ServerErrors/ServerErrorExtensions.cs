using Microsoft.Extensions.DependencyInjection;
using Chik.Exams.Data;

namespace Chik.Exams;

public static class ServerErrorExtensions
{
    public static IServiceCollection AddServerError(this IServiceCollection services)
    {
        services.TrackScoped<IServerErrorRepository, ServerErrorRepository>();
        services.TrackScoped<IServerErrorService, ServerErrorService>();
        return services;
    }
}
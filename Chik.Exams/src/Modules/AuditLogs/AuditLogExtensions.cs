using Chik.Exams.Data;

namespace Chik.Exams;

public static class AuditLogExtensions
{
    public static IServiceCollection AddAuditLog(this IServiceCollection services)
    {
        services.TrackScoped<IAuditLogRepository, AuditLogRepository>();
        services.TrackScoped<IAuditLogService, AuditLogService>();
        return services;
    }
}
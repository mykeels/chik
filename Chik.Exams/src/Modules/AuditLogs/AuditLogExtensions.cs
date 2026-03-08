using Chik.Exams.Data;

namespace Chik.Exams;

public static class AuditLogExtensions
{
    public static IServiceCollection AddAuditLog(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        return services;
    }
}
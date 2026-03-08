using Chik.Exams.Mails;

namespace Chik.Exams;

public static class MailsExtensions
{
    public static IServiceCollection AddEmailService(this IServiceCollection services, EmailCredentials credentials)
    {
        services.AddSingleton(credentials);
        services.AddSingleton<IEmailService, EmailService>();
        services.AddEmailTemplates();
        return services;
    }

    private static IServiceCollection AddEmailTemplates(this IServiceCollection services)
    {
        var templates = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IEmailTemplate).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
            .ToList();
        foreach (var template in templates)
        {
            services.AddScoped(template, template);
        }
        return services;
    }
}
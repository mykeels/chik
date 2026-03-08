using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory;
using Spectre.Console;

namespace Chik.Exams.Data;

public static class ChikExamsDbContextExtensions
{
    public static IServiceCollection AddChikExamsDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ChikExamsDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            }, ServiceLifetime.Transient);
        services.AddDbContextFactory<ChikExamsDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            });
        return services;
    }

    public static IServiceCollection AddInMemoryChikExamsDbContext(this IServiceCollection services)
    {
        // Use InMemory provider for OpenAPI generation - no native dependencies required
        services.AddDbContext<ChikExamsDbContext>(options =>
            {
                options.UseInMemoryDatabase("ChikExamsOpenApi");
                options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            }, ServiceLifetime.Scoped);
        
        services.AddDbContextFactory<ChikExamsDbContext>(options =>
            {
                options.UseInMemoryDatabase("ChikExamsOpenApi");
                options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            }, ServiceLifetime.Scoped);
        
        return services;
    }

    public static string GetDBPasswordFromConsole()
    {
        string password = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter DB password: ")
                .Secret('*'));
        return password;
    }
}
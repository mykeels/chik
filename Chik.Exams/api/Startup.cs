using Chik.Exams.Data;
using Microsoft.EntityFrameworkCore;
using Chik.Exams;
using System.Reflection;
using ZiggyCreatures.Caching.Fusion;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.CommandLine;

namespace Chik.Exams.Api;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly bool _isExtractingOpenApi;
    private readonly bool _isReplMode;
    private readonly bool _shouldConnectToDb;

    public RootCommand RootCommand { get; private set; }

    public Startup(IConfiguration configuration, string[]? args = null)
    {
        args ??= [];
        _configuration = configuration;
        _isExtractingOpenApi = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAPI"));
        _isReplMode = args.Contains("repl");
        var rootCommand = Commands.Setup();
        var rootCommandParseResult = rootCommand.Parse(args);
        var dbOption = rootCommand.Options.FirstOrDefault(o => o.Name == "db");
        _shouldConnectToDb = dbOption is not null && rootCommandParseResult.GetValueForOption(
            dbOption
        ) is bool dbOptionValue && dbOptionValue;
        RootCommand = rootCommand;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IConfiguration>(_configuration);

        services.AddLoggingServices();

        services.AddSingleton<RemoteEnvironment>();
        services.AddMemoryCache();
        services.AddFusionCache()
            .WithDefaultEntryOptions(opt => opt.SetDuration(TimeSpan.FromMinutes(5)));

        services.AddAuthenticationServices(_configuration);
        services.AddRoleAuthorization();
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = false;
        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithOrigins(
                        "http://localhost",
                        "http://localhost:5173",
                        "http://localhost:6006",
                        "https://www.chik.ng",
                        "https://chik.ng",
                        "https://beta.chik.ng",
                        "https://exams.chik.ng"
                    );
            });
        });
        services.AddControllers();
        services.AddChikExams(_configuration);

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddSwaggerGen(options =>
        {
            var entryAssembly = typeof(Program).Assembly;
            string version = (entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0").Split('+')[0];
            options.SwaggerDoc("v1", new OpenApiInfo { 
                Title = "Chik.Exams", 
                Version = version,
                Description = "Chik.Exams API",
                Contact = new OpenApiContact
                {
                    Name = "Chik.Exams",
                    Email = "support@chik.ng"
                }
            });
            options.CustomOperationIds(e =>
                $"{e.ActionDescriptor.RouteValues["controller"]}_{e.ActionDescriptor.RouteValues["action"]}"
            );
            options.CustomSchemaIds(type => type.FullName?.Replace("+", ".").Replace("Chik.Exams.", ""));
            options.DocumentFilter<AdditionalPropertiesDocumentFilter>();
            options.SchemaFilter<EnumDescriptionSchemaFilter>();
            options.SchemaFilter<NullableReferenceSchemaFilter>();
            options.SchemaFilter<NullableEnumSchemaFilter>();
            options.NonNullableReferenceTypesAsRequired();
        });
        services.AddHealthChecks()
            .AddDbContextCheck<ChikExamsDbContext>();

        // Add Entity Framework DbContext
        if (!_isExtractingOpenApi && _shouldConnectToDb)
        {
            string connectionString = _isReplMode ? ChikExamsDbContext.GetConnectionString(
                ChikExamsDbContextExtensions.GetDBPasswordFromConsole()
            ) : _configuration.GetConnectionString("db") ?? throw new InvalidOperationException("DB Connection string is not set");
            services.AddChikExamsDbContext(connectionString);
        }
        else
        {
            services.AddInMemoryChikExamsDbContext();
        }
    }

    public async Task Configure(WebApplication app)
    {
        var scoped = app.Services.CreateScope().ServiceProvider;
        Provider.SetInstance(scoped);
        scoped.AssertTrackedServices();

        // Ensure database is migrated
        if (!_isExtractingOpenApi)
        {
            app.UseGlobalExceptionHandler();
            if (!_isReplMode)
            {
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ChikExamsDbContext>();
                    if (_shouldConnectToDb)
                    {
                        await dbContext.Database.MigrateAsync();
                    }
                    await Seeder.Seed(scope.ServiceProvider);
                }
            }
            else
            {
               using (var scope = app.Services.CreateScope())
               {
                    await Seeder.Seed(scope.ServiceProvider);
               }
            }
            if (!_shouldConnectToDb)
            {
                DryRun.UseDryRun();
            }
        }

        app.UseCors();
        // app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        // Map default endpoints (health checks, etc.)
        app.MapDefaultEndpoints();
        app.MapControllers();
    }
}


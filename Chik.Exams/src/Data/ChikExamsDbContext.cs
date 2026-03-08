using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Chik.Exams.Data;

public class ChikExamsDbContext : DbContext
{
    public ChikExamsDbContext(DbContextOptions<ChikExamsDbContext> options)
        : base(options)
    {
    }

    public static string GetConnectionString(string password, RemoteEnvironment? remoteEnvironment = null, string? database = "under4games")
    {
        remoteEnvironment ??= Provider.GetService<RemoteEnvironment>() ?? new RemoteEnvironment();
        string host = remoteEnvironment.Environment == RemoteEnvironment.Production ? "host.docker.internal" : "localhost";
        return $"Host={host};Port=5432;Database={database};Username=postgres;Password={password}";
    }

    // Add your DbSet properties here for your entities
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizQuestionType> QuizQuestionTypes { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<ExamAnswer> ExamAnswers { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<IpAddressLocation> IpAddressLocations { get; set; }
    public DbSet<Login> Logins { get; set; }
    // public DbSet<Player> Players { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure your entity relationships and constraints here
        
        AddQuiz(modelBuilder);
        AddQuizQuestion(modelBuilder);
        AddQuizQuestionType(modelBuilder);
        AddExam(modelBuilder);
        AddExamAnswer(modelBuilder);
        AddAuditLog(modelBuilder);
        AddUser(modelBuilder);
        AddIpAddressLocation(modelBuilder);
        AddLogin(modelBuilder);
        // Relationships
        AddQuizRelationships(modelBuilder);
        AddQuizQuestionRelationships(modelBuilder);
        AddQuizQuestionTypeRelationships(modelBuilder);
        AddExamRelationships(modelBuilder);
        AddExamAnswerRelationships(modelBuilder);
        AddAuditLogRelationships(modelBuilder);
        AddUserRelationships(modelBuilder);
        AddIpAddressLocationRelationships(modelBuilder);
        AddLoginRelationships(modelBuilder);
        ApplyUTCToDateTimeProperties(modelBuilder);
    }

    private void ApplyUTCToDateTimeProperties(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties()
                .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
            {
                property.SetValueConverter(
                    new ValueConverter<DateTime, DateTime>(
                        v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                    )
                );
            }
        }
    }
}
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

    // DbSet properties for entities
    public DbSet<QuizDbo> Quizzes { get; set; }
    public DbSet<QuizQuestionDbo> QuizQuestions { get; set; }
    public DbSet<QuizQuestionTypeDbo> QuizQuestionTypes { get; set; }
    public DbSet<ExamDbo> Exams { get; set; }
    public DbSet<ExamAnswerDbo> ExamAnswers { get; set; }
    public DbSet<AuditLogDbo> AuditLogs { get; set; }
    public DbSet<UserDbo> Users { get; set; }
    public DbSet<IpAddressLocationDbo> IpAddressLocations { get; set; }
    public DbSet<LoginDbo> Logins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entities
        AddUser(modelBuilder);
        AddQuiz(modelBuilder);
        AddQuizQuestion(modelBuilder);
        AddQuizQuestionType(modelBuilder);
        AddExam(modelBuilder);
        AddExamAnswer(modelBuilder);
        AddAuditLog(modelBuilder);
        AddIpAddressLocation(modelBuilder);
        AddLogin(modelBuilder);

        // Configure relationships
        AddUserRelationships(modelBuilder);
        AddQuizRelationships(modelBuilder);
        AddQuizQuestionRelationships(modelBuilder);
        AddQuizQuestionTypeRelationships(modelBuilder);
        AddExamRelationships(modelBuilder);
        AddExamAnswerRelationships(modelBuilder);
        AddAuditLogRelationships(modelBuilder);
        AddIpAddressLocationRelationships(modelBuilder);
        AddLoginRelationships(modelBuilder);

        ApplyUTCToDateTimeProperties(modelBuilder);
    }

    #region Entity Configurations

    private void AddUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserDbo>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Username).HasColumnName("username").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Password).HasColumnName("password").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Roles).HasColumnName("roles").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Username).IsUnique();
        });
    }

    private void AddQuiz(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuizDbo>(entity =>
        {
            entity.ToTable("quizzes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasColumnName("description").IsRequired();
            entity.Property(e => e.CreatorId).HasColumnName("creator_id").IsRequired();
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });
    }

    private void AddQuizQuestion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuizQuestionDbo>(entity =>
        {
            entity.ToTable("quiz_questions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.QuizId).HasColumnName("quiz_id").IsRequired();
            entity.Property(e => e.Prompt).HasColumnName("prompt").IsRequired();
            entity.Property(e => e.TypeId).HasColumnName("type_id").IsRequired();
            entity.Property(e => e.Properties).HasColumnName("properties").IsRequired();
            entity.Property(e => e.Score).HasColumnName("score").IsRequired();
            entity.Property(e => e.Order).HasColumnName("order").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.DeactivatedAt).HasColumnName("deactivated_at");

            entity.HasIndex(e => new { e.QuizId, e.Order });
        });
    }

    private void AddQuizQuestionType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuizQuestionTypeDbo>(entity =>
        {
            entity.ToTable("quiz_question_types");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnName("description").IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Name).IsUnique();
        });
    }

    private void AddExam(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExamDbo>(entity =>
        {
            entity.ToTable("exams");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.QuizId).HasColumnName("quiz_id").IsRequired();
            entity.Property(e => e.CreatorId).HasColumnName("creator_id").IsRequired();
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.EndedAt).HasColumnName("ended_at");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.ExaminerId).HasColumnName("examiner_id");
            entity.Property(e => e.ExaminerComment).HasColumnName("examiner_comment");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => new { e.UserId, e.QuizId });
            entity.HasIndex(e => e.CreatorId);
        });
    }

    private void AddExamAnswer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExamAnswerDbo>(entity =>
        {
            entity.ToTable("exam_answers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.ExamId).HasColumnName("exam_id").IsRequired();
            entity.Property(e => e.QuestionId).HasColumnName("question_id").IsRequired();
            entity.Property(e => e.Answer).HasColumnName("answer").IsRequired();
            entity.Property(e => e.AutoScore).HasColumnName("auto_score");
            entity.Property(e => e.ExaminerScore).HasColumnName("examiner_score");
            entity.Property(e => e.ExaminerId).HasColumnName("examiner_id");
            entity.Property(e => e.ExaminerComment).HasColumnName("examiner_comment");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => new { e.ExamId, e.QuestionId }).IsUnique();
        });
    }

    private void AddAuditLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogDbo>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.Service).HasColumnName("service").IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityId).HasColumnName("entity_id").IsRequired();
            entity.Property(e => e.Properties).HasColumnName("properties").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

            entity.HasIndex(e => new { e.Service, e.EntityId });
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private void AddIpAddressLocation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IpAddressLocationDbo>(entity =>
        {
            entity.ToTable("ip_address_locations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").IsRequired().HasMaxLength(45);
            entity.Property(e => e.CountryCode).HasColumnName("country_code").IsRequired().HasMaxLength(10);

            entity.HasIndex(e => e.IpAddress).IsUnique();
        });
    }

    private void AddLogin(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoginDbo>(entity =>
        {
            entity.ToTable("logins");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.IpAddressLocationId).HasColumnName("ip_address_location_id").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    #endregion

    #region Relationship Configurations

    private void AddUserRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserDbo>(entity =>
        {
            // User -> Quizzes (created)
            entity.HasMany(e => e.CreatedQuizzes)
                .WithOne(e => e.Creator)
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Exams (created)
            entity.HasMany(e => e.CreatedExams)
                .WithOne(e => e.Creator)
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Exams (taken)
            entity.HasMany(e => e.TakenExams)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Exams (examined)
            entity.HasMany(e => e.ExaminedExams)
                .WithOne(e => e.Examiner)
                .HasForeignKey(e => e.ExaminerId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> ExamAnswers (examined)
            entity.HasMany(e => e.ExaminedAnswers)
                .WithOne(e => e.Examiner)
                .HasForeignKey(e => e.ExaminerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void AddQuizRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuizDbo>(entity =>
        {
            // Quiz -> Questions
            entity.HasMany(e => e.Questions)
                .WithOne(e => e.Quiz)
                .HasForeignKey(e => e.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quiz -> Exams
            entity.HasMany(e => e.Exams)
                .WithOne(e => e.Quiz)
                .HasForeignKey(e => e.QuizId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void AddQuizQuestionRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuizQuestionDbo>(entity =>
        {
            // QuizQuestion -> ExamAnswers
            entity.HasMany(e => e.ExamAnswers)
                .WithOne(e => e.Question)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void AddQuizQuestionTypeRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuizQuestionTypeDbo>(entity =>
        {
            // QuestionType -> Questions
            entity.HasMany(e => e.Questions)
                .WithOne(e => e.Type)
                .HasForeignKey(e => e.TypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void AddExamRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExamDbo>(entity =>
        {
            // Exam -> Answers
            entity.HasMany(e => e.Answers)
                .WithOne(e => e.Exam)
                .HasForeignKey(e => e.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void AddExamAnswerRelationships(ModelBuilder modelBuilder)
    {
        // Relationships already configured via other entities
    }

    private void AddAuditLogRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogDbo>(entity =>
        {
            // AuditLog -> User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void AddIpAddressLocationRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IpAddressLocationDbo>(entity =>
        {
            // IpAddressLocation -> Logins
            entity.HasMany(e => e.Logins)
                .WithOne(e => e.IpAddressLocation)
                .HasForeignKey(e => e.IpAddressLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void AddLoginRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoginDbo>(entity =>
        {
            // Login -> User
            entity.HasOne(e => e.User)
                .WithMany(e => e.Logins)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    #endregion

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

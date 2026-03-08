using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Chik.Exams.Tests;

public static class TestUtils
{
    public static TimeProvider TimeProvider
    {
        get
        {
            var timeProvider = new Mock<TimeProvider>();
            timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
            return timeProvider.Object;
        }
    }

    public static User SampleUser => new User(
        Id: 1,
        "user@chik.com",
        (int)UserRole.Admin,
        DateTime.UtcNow,
        DateTime.UtcNow
    );

    public static void SetupServiceProvider(Action<ServiceCollection> setup)
    {
        var services = new ServiceCollection();
        setup(services);
        var provider = services.BuildServiceProvider();
        Provider.SetInstance(provider);
    }

    /// <summary>
    /// Creates a test user in the database and returns its ID.
    /// </summary>
    public static async Task<UserDbo> CreateTestUser(IDbContextFactory<ChikExamsDbContext> factory, string username = "testuser")
    {
        using var dbContext = factory.CreateDbContext();
        var user = new UserDbo
        {
            Username = username,
            Password = "password123",
            Roles = (int)UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Creates a test quiz in the database and returns it.
    /// </summary>
    public static async Task<QuizDbo> CreateTestQuiz(IDbContextFactory<ChikExamsDbContext> factory, long creatorId, string title = "Test Quiz")
    {
        using var dbContext = factory.CreateDbContext();
        var quiz = new QuizDbo
        {
            Title = title,
            Description = "Test Description",
            CreatorId = creatorId,
            CreatedAt = DateTime.UtcNow
        };
        await dbContext.Quizzes.AddAsync(quiz);
        await dbContext.SaveChangesAsync();
        return quiz;
    }

    /// <summary>
    /// Creates a test quiz question type in the database and returns it.
    /// </summary>
    public static async Task<QuizQuestionTypeDbo> CreateTestQuizQuestionType(IDbContextFactory<ChikExamsDbContext> factory, string name = "single-choice")
    {
        using var dbContext = factory.CreateDbContext();
        var questionType = new QuizQuestionTypeDbo
        {
            Name = name,
            Description = "Test question type",
            CreatedAt = DateTime.UtcNow
        };
        await dbContext.QuizQuestionTypes.AddAsync(questionType);
        await dbContext.SaveChangesAsync();
        return questionType;
    }

    /// <summary>
    /// Creates a test exam in the database and returns it.
    /// </summary>
    public static async Task<ExamDbo> CreateTestExam(IDbContextFactory<ChikExamsDbContext> factory, long userId, long quizId, long creatorId)
    {
        using var dbContext = factory.CreateDbContext();
        var exam = new ExamDbo
        {
            UserId = userId,
            QuizId = quizId,
            CreatorId = creatorId,
            CreatedAt = DateTime.UtcNow
        };
        await dbContext.Exams.AddAsync(exam);
        await dbContext.SaveChangesAsync();
        return exam;
    }

    /// <summary>
    /// Creates a test quiz question in the database and returns it.
    /// </summary>
    public static async Task<QuizQuestionDbo> CreateTestQuizQuestion(IDbContextFactory<ChikExamsDbContext> factory, long quizId, int typeId, string prompt = "Test question?")
    {
        using var dbContext = factory.CreateDbContext();
        var question = new QuizQuestionDbo
        {
            QuizId = quizId,
            Prompt = prompt,
            TypeId = typeId,
            Properties = "{}",
            Score = 10,
            Order = 1,
            CreatedAt = DateTime.UtcNow
        };
        await dbContext.QuizQuestions.AddAsync(question);
        await dbContext.SaveChangesAsync();
        return question;
    }
}
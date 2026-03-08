namespace Chik.Exams.Tests.Services;

[TestFixture]
public class QuizQuestionServiceTests
{
    private Mock<IQuizQuestionRepository> _questionRepositoryMock = null!;
    private Mock<IQuizRepository> _quizRepositoryMock = null!;
    private Mock<IAuditLogService> _auditLogServiceMock = null!;
    private Mock<ILogger<QuizQuestionService>> _loggerMock = null!;
    private QuizQuestionService _service = null!;

    private User AdminUser => new(1, "admin", [UserRole.Admin], DateTime.UtcNow, null);
    private User TeacherUser => new(2, "teacher", [UserRole.Teacher], DateTime.UtcNow, null);
    private User OtherTeacher => new(4, "other_teacher", [UserRole.Teacher], DateTime.UtcNow, null);
    private User StudentUser => new(3, "student", [UserRole.Student], DateTime.UtcNow, null);

    private QuizDbo OwnedQuiz => new() { Id = 10, Title = "My Quiz", CreatorId = 2, CreatedAt = DateTime.UtcNow };
    private QuizDbo ExaminedQuiz => new() { Id = 11, Title = "Examined Quiz", CreatorId = 1, ExaminerId = 2, CreatedAt = DateTime.UtcNow };
    private QuizDbo OtherQuiz => new() { Id = 12, Title = "Other Quiz", CreatorId = 4, CreatedAt = DateTime.UtcNow };

    [SetUp]
    public void SetUp()
    {
        _questionRepositoryMock = new Mock<IQuizQuestionRepository>();
        _quizRepositoryMock = new Mock<IQuizRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _loggerMock = new Mock<ILogger<QuizQuestionService>>();
        _service = new QuizQuestionService(
            _questionRepositoryMock.Object,
            _quizRepositoryMock.Object,
            _auditLogServiceMock.Object,
            _loggerMock.Object);
    }

    #region Create Tests

    [Test]
    public async Task Create_AsAdmin_ShouldSucceed()
    {
        // Arrange
        var createQuestion = new QuizQuestion.Create(10, "What is 2+2?", 1, "{}", 10, 1);
        var createdDbo = new QuizQuestionDbo { Id = 100, QuizId = 10, Prompt = "What is 2+2?", Score = 10, Order = 1, CreatedAt = DateTime.UtcNow };
        _questionRepositoryMock.Setup(r => r.Create(createQuestion)).ReturnsAsync(createdDbo);

        // Act
        var result = await _service.Create(AdminUser, createQuestion);

        // Assert
        Assert.That(result.Prompt, Is.EqualTo("What is 2+2?"));
        _auditLogServiceMock.Verify(a => a.Create(It.IsAny<User>(), It.IsAny<AuditLog.Create<QuizQuestion.Create>>()), Times.Once);
    }

    [Test]
    public async Task Create_AsTeacher_OwnQuiz_ShouldSucceed()
    {
        // Arrange
        var createQuestion = new QuizQuestion.Create(10, "What is 3+3?", 1, "{}", 10, 1);
        var createdDbo = new QuizQuestionDbo { Id = 100, QuizId = 10, Prompt = "What is 3+3?", Score = 10, Order = 1, CreatedAt = DateTime.UtcNow };
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(OwnedQuiz);
        _questionRepositoryMock.Setup(r => r.Create(createQuestion)).ReturnsAsync(createdDbo);

        // Act
        var result = await _service.Create(TeacherUser, createQuestion);

        // Assert
        Assert.That(result.Prompt, Is.EqualTo("What is 3+3?"));
    }

    [Test]
    public async Task Create_AsTeacher_ExaminedQuiz_ShouldSucceed()
    {
        // Arrange
        var createQuestion = new QuizQuestion.Create(11, "What is 4+4?", 1, "{}", 10, 1);
        var createdDbo = new QuizQuestionDbo { Id = 100, QuizId = 11, Prompt = "What is 4+4?", Score = 10, Order = 1, CreatedAt = DateTime.UtcNow };
        _quizRepositoryMock.Setup(r => r.Get(11, false)).ReturnsAsync(ExaminedQuiz);
        _questionRepositoryMock.Setup(r => r.Create(createQuestion)).ReturnsAsync(createdDbo);

        // Act
        var result = await _service.Create(TeacherUser, createQuestion);

        // Assert
        Assert.That(result.Prompt, Is.EqualTo("What is 4+4?"));
    }

    [Test]
    public void Create_AsTeacher_OtherQuiz_ShouldThrow()
    {
        // Arrange
        var createQuestion = new QuizQuestion.Create(12, "What is 5+5?", 1, "{}", 10, 1);
        _quizRepositoryMock.Setup(r => r.Get(12, false)).ReturnsAsync(OtherQuiz);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Create(TeacherUser, createQuestion));
        Assert.That(ex.Message, Does.Contain("only manage questions for quizzes you created"));
    }

    [Test]
    public void Create_AsStudent_ShouldThrow()
    {
        // Arrange
        var createQuestion = new QuizQuestion.Create(10, "What is 6+6?", 1, "{}", 10, 1);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Create(StudentUser, createQuestion));
        Assert.That(ex.Message, Does.Contain("Only Admin or Teacher"));
    }

    #endregion

    #region GetByQuizId Tests

    [Test]
    public async Task GetByQuizId_AsAdmin_ShouldSucceed()
    {
        // Arrange
        var questions = new List<QuizQuestionDbo>
        {
            new() { Id = 1, QuizId = 10, Prompt = "Q1", Score = 10, Order = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, QuizId = 10, Prompt = "Q2", Score = 10, Order = 2, CreatedAt = DateTime.UtcNow }
        };
        _questionRepositoryMock.Setup(r => r.GetByQuizId(10, false)).ReturnsAsync(questions);

        // Act
        var result = await _service.GetByQuizId(AdminUser, 10);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetByQuizId_AsTeacher_OwnQuiz_ShouldSucceed()
    {
        // Arrange
        var questions = new List<QuizQuestionDbo>
        {
            new() { Id = 1, QuizId = 10, Prompt = "Q1", Score = 10, Order = 1, CreatedAt = DateTime.UtcNow }
        };
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(OwnedQuiz);
        _questionRepositoryMock.Setup(r => r.GetByQuizId(10, false)).ReturnsAsync(questions);

        // Act
        var result = await _service.GetByQuizId(TeacherUser, 10);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
    }

    #endregion

    #region Update Tests

    [Test]
    public async Task Update_AsTeacher_OwnQuiz_ShouldSucceed()
    {
        // Arrange
        var existingQuestion = new QuizQuestionDbo { Id = 100, QuizId = 10, Prompt = "Old prompt", Score = 10, Order = 1, CreatedAt = DateTime.UtcNow };
        var updateQuestion = new QuizQuestion.Update(100, Prompt: "New prompt");
        var updatedDbo = new QuizQuestionDbo { Id = 100, QuizId = 10, Prompt = "New prompt", Score = 10, Order = 1, CreatedAt = DateTime.UtcNow };
        
        _questionRepositoryMock.Setup(r => r.Get(100)).ReturnsAsync(existingQuestion);
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(OwnedQuiz);
        _questionRepositoryMock.Setup(r => r.Update(100, updateQuestion)).ReturnsAsync(updatedDbo);

        // Act
        var result = await _service.Update(TeacherUser, updateQuestion);

        // Assert
        Assert.That(result.Prompt, Is.EqualTo("New prompt"));
    }

    [Test]
    public void Update_QuestionNotFound_ShouldThrow()
    {
        // Arrange
        var updateQuestion = new QuizQuestion.Update(999, Prompt: "New prompt");
        _questionRepositoryMock.Setup(r => r.Get(999)).ReturnsAsync((QuizQuestionDbo?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _service.Update(AdminUser, updateQuestion));
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    #endregion

    #region Deactivate Tests

    [Test]
    public async Task Deactivate_AsTeacher_OwnQuiz_ShouldSucceed()
    {
        // Arrange
        var existingQuestion = new QuizQuestionDbo { Id = 100, QuizId = 10, Prompt = "Question", Score = 10, Order = 1, CreatedAt = DateTime.UtcNow };
        _questionRepositoryMock.Setup(r => r.Get(100)).ReturnsAsync(existingQuestion);
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(OwnedQuiz);
        _questionRepositoryMock.Setup(r => r.Deactivate(100)).Returns(Task.CompletedTask);

        // Act
        await _service.Deactivate(TeacherUser, 100);

        // Assert
        _questionRepositoryMock.Verify(r => r.Deactivate(100), Times.Once);
        _auditLogServiceMock.Verify(a => a.Create(It.IsAny<User>(), It.IsAny<AuditLog.Create<object>>()), Times.Once);
    }

    #endregion

    #region Reactivate Tests

    [Test]
    public async Task Reactivate_AsTeacher_OwnQuiz_ShouldSucceed()
    {
        // Arrange
        var existingQuestion = new QuizQuestionDbo { Id = 100, QuizId = 10, Prompt = "Question", Score = 10, Order = 1, DeactivatedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
        _questionRepositoryMock.Setup(r => r.Get(100)).ReturnsAsync(existingQuestion);
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(OwnedQuiz);
        _questionRepositoryMock.Setup(r => r.Reactivate(100)).Returns(Task.CompletedTask);

        // Act
        await _service.Reactivate(TeacherUser, 100);

        // Assert
        _questionRepositoryMock.Verify(r => r.Reactivate(100), Times.Once);
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_AsAdmin_ShouldSucceed()
    {
        // Arrange
        var existingQuestion = new QuizQuestionDbo { Id = 100, QuizId = 10, Prompt = "Question", Score = 10, Order = 1, CreatedAt = DateTime.UtcNow };
        _questionRepositoryMock.Setup(r => r.Get(100)).ReturnsAsync(existingQuestion);
        _questionRepositoryMock.Setup(r => r.Delete(100)).Returns(Task.CompletedTask);

        // Act
        await _service.Delete(AdminUser, 100);

        // Assert
        _questionRepositoryMock.Verify(r => r.Delete(100), Times.Once);
    }

    [Test]
    public void Delete_AsTeacher_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Delete(TeacherUser, 100));
        Assert.That(ex.Message, Does.Contain("Only Admin can permanently delete"));
    }

    #endregion

    #region ReorderQuestions Tests

    [Test]
    public async Task ReorderQuestions_AsTeacher_OwnQuiz_ShouldSucceed()
    {
        // Arrange
        var newOrder = new List<long> { 3, 1, 2 };
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(OwnedQuiz);
        _questionRepositoryMock.Setup(r => r.ReorderQuestions(10, newOrder)).Returns(Task.CompletedTask);

        // Act
        await _service.ReorderQuestions(TeacherUser, 10, newOrder);

        // Assert
        _questionRepositoryMock.Verify(r => r.ReorderQuestions(10, newOrder), Times.Once);
    }

    #endregion
}

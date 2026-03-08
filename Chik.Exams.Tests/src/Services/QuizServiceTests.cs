namespace Chik.Exams.Tests.Services;

[TestFixture]
public class QuizServiceTests
{
    private Mock<IQuizRepository> _quizRepositoryMock = null!;
    private Mock<IAuditLogService> _auditLogServiceMock = null!;
    private Mock<ILogger<QuizService>> _loggerMock = null!;
    private QuizService _service = null!;

    private User AdminUser => new(1, "admin", [UserRole.Admin], DateTime.UtcNow, null);
    private User TeacherUser => new(2, "teacher", [UserRole.Teacher], DateTime.UtcNow, null);
    private User OtherTeacher => new(4, "other_teacher", [UserRole.Teacher], DateTime.UtcNow, null);
    private User StudentUser => new(3, "student", [UserRole.Student], DateTime.UtcNow, null);

    [SetUp]
    public void SetUp()
    {
        _quizRepositoryMock = new Mock<IQuizRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _loggerMock = new Mock<ILogger<QuizService>>();
        _service = new QuizService(_quizRepositoryMock.Object, _auditLogServiceMock.Object, _loggerMock.Object);
    }

    #region Create Tests

    [Test]
    public async Task Create_AsAdmin_ShouldSucceed()
    {
        // Arrange
        var createQuiz = new Quiz.Create("Test Quiz", "Description", 1);
        var createdDbo = new QuizDbo { Id = 10, Title = "Test Quiz", Description = "Description", CreatorId = 1, CreatedAt = DateTime.UtcNow };
        _quizRepositoryMock.Setup(r => r.Create(createQuiz)).ReturnsAsync(createdDbo);

        // Act
        var result = await _service.Create(AdminUser, createQuiz);

        // Assert
        Assert.That(result.Title, Is.EqualTo("Test Quiz"));
        _auditLogServiceMock.Verify(a => a.Create(It.IsAny<User>(), It.IsAny<AuditLog.Create<Quiz.Create>>()), Times.Once);
    }

    [Test]
    public async Task Create_AsTeacher_ShouldSucceed()
    {
        // Arrange
        var createQuiz = new Quiz.Create("Teacher Quiz", "Description", 2);
        var createdDbo = new QuizDbo { Id = 10, Title = "Teacher Quiz", Description = "Description", CreatorId = 2, CreatedAt = DateTime.UtcNow };
        _quizRepositoryMock.Setup(r => r.Create(createQuiz)).ReturnsAsync(createdDbo);

        // Act
        var result = await _service.Create(TeacherUser, createQuiz);

        // Assert
        Assert.That(result.Title, Is.EqualTo("Teacher Quiz"));
    }

    [Test]
    public void Create_AsStudent_ShouldThrow()
    {
        // Arrange
        var createQuiz = new Quiz.Create("Student Quiz", "Description", 3);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Create(StudentUser, createQuiz));
        Assert.That(ex.Message, Does.Contain("Only Admin or Teacher"));
    }

    #endregion

    #region Get Tests

    [Test]
    public async Task Get_AsAdmin_AnyQuiz_ShouldSucceed()
    {
        // Arrange
        var quizDbo = new QuizDbo { Id = 10, Title = "Any Quiz", Description = "Desc", CreatorId = 2, CreatedAt = DateTime.UtcNow };
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(quizDbo);

        // Act
        var result = await _service.Get(AdminUser, 10);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Is.EqualTo("Any Quiz"));
    }

    [Test]
    public async Task Get_AsTeacher_OwnQuiz_ShouldSucceed()
    {
        // Arrange
        var quizDbo = new QuizDbo { Id = 10, Title = "My Quiz", Description = "Desc", CreatorId = 2, CreatedAt = DateTime.UtcNow };
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(quizDbo);

        // Act
        var result = await _service.Get(TeacherUser, 10);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task Get_AsTeacher_ExaminedQuiz_ShouldSucceed()
    {
        // Arrange
        var quizDbo = new QuizDbo { Id = 10, Title = "Examined Quiz", Description = "Desc", CreatorId = 1, ExaminerId = 2, CreatedAt = DateTime.UtcNow };
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(quizDbo);

        // Act
        var result = await _service.Get(TeacherUser, 10);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void Get_AsTeacher_OtherTeacherQuiz_ShouldThrow()
    {
        // Arrange
        var quizDbo = new QuizDbo { Id = 10, Title = "Other Quiz", Description = "Desc", CreatorId = 4, CreatedAt = DateTime.UtcNow };
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(quizDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Get(TeacherUser, 10));
        Assert.That(ex.Message, Does.Contain("only view quizzes you created"));
    }

    [Test]
    public void Get_AsStudent_ShouldThrow()
    {
        // Arrange
        var quizDbo = new QuizDbo { Id = 10, Title = "Any Quiz", Description = "Desc", CreatorId = 2, CreatedAt = DateTime.UtcNow };
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(quizDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Get(StudentUser, 10));
        Assert.That(ex.Message, Does.Contain("Only Admin or Teacher"));
    }

    #endregion

    #region Update Tests

    [Test]
    public async Task Update_AsAdmin_AnyQuiz_ShouldSucceed()
    {
        // Arrange
        var quizDbo = new QuizDbo { Id = 10, Title = "Original", Description = "Desc", CreatorId = 2, CreatedAt = DateTime.UtcNow };
        var updateQuiz = new Quiz.Update(10, Title: "Updated Title");
        var updatedDbo = new QuizDbo { Id = 10, Title = "Updated Title", Description = "Desc", CreatorId = 2, CreatedAt = DateTime.UtcNow };
        
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(quizDbo);
        _quizRepositoryMock.Setup(r => r.Update(10, updateQuiz)).ReturnsAsync(updatedDbo);

        // Act
        var result = await _service.Update(AdminUser, updateQuiz);

        // Assert
        Assert.That(result.Title, Is.EqualTo("Updated Title"));
    }

    [Test]
    public async Task Update_AsTeacher_OwnQuiz_ShouldSucceed()
    {
        // Arrange
        var quizDbo = new QuizDbo { Id = 10, Title = "Original", Description = "Desc", CreatorId = 2, CreatedAt = DateTime.UtcNow };
        var updateQuiz = new Quiz.Update(10, Title: "My Updated Title");
        var updatedDbo = new QuizDbo { Id = 10, Title = "My Updated Title", Description = "Desc", CreatorId = 2, CreatedAt = DateTime.UtcNow };
        
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(quizDbo);
        _quizRepositoryMock.Setup(r => r.Update(10, updateQuiz)).ReturnsAsync(updatedDbo);

        // Act
        var result = await _service.Update(TeacherUser, updateQuiz);

        // Assert
        Assert.That(result.Title, Is.EqualTo("My Updated Title"));
    }

    [Test]
    public void Update_AsTeacher_OtherTeacherQuiz_ShouldThrow()
    {
        // Arrange
        var quizDbo = new QuizDbo { Id = 10, Title = "Other Quiz", Description = "Desc", CreatorId = 4, CreatedAt = DateTime.UtcNow };
        var updateQuiz = new Quiz.Update(10, Title: "Hacked Title");
        
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(quizDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Update(TeacherUser, updateQuiz));
        Assert.That(ex.Message, Does.Contain("only update quizzes you created"));
    }

    [Test]
    public void Update_QuizNotFound_ShouldThrow()
    {
        // Arrange
        var updateQuiz = new Quiz.Update(999, Title: "Title");
        _quizRepositoryMock.Setup(r => r.Get(999, false)).ReturnsAsync((QuizDbo?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _service.Update(AdminUser, updateQuiz));
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_AsAdmin_AnyQuiz_ShouldSucceed()
    {
        // Arrange
        var quizDbo = new QuizDbo { Id = 10, Title = "Quiz", Description = "Desc", CreatorId = 2, CreatedAt = DateTime.UtcNow };
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(quizDbo);
        _quizRepositoryMock.Setup(r => r.Delete(10)).Returns(Task.CompletedTask);

        // Act
        await _service.Delete(AdminUser, 10);

        // Assert
        _quizRepositoryMock.Verify(r => r.Delete(10), Times.Once);
    }

    [Test]
    public async Task Delete_AsTeacher_OwnQuiz_ShouldSucceed()
    {
        // Arrange
        var quizDbo = new QuizDbo { Id = 10, Title = "My Quiz", Description = "Desc", CreatorId = 2, CreatedAt = DateTime.UtcNow };
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(quizDbo);
        _quizRepositoryMock.Setup(r => r.Delete(10)).Returns(Task.CompletedTask);

        // Act
        await _service.Delete(TeacherUser, 10);

        // Assert
        _quizRepositoryMock.Verify(r => r.Delete(10), Times.Once);
    }

    [Test]
    public void Delete_AsTeacher_OtherTeacherQuiz_ShouldThrow()
    {
        // Arrange
        var quizDbo = new QuizDbo { Id = 10, Title = "Other Quiz", Description = "Desc", CreatorId = 4, CreatedAt = DateTime.UtcNow };
        _quizRepositoryMock.Setup(r => r.Get(10, false)).ReturnsAsync(quizDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Delete(TeacherUser, 10));
        Assert.That(ex.Message, Does.Contain("only delete quizzes you created"));
    }

    #endregion

    #region Search Tests

    [Test]
    public async Task Search_AsAdmin_ShouldReturnAll()
    {
        // Arrange
        var quizzes = new List<QuizDbo>
        {
            new() { Id = 1, Title = "Quiz 1", Description = "Desc", CreatorId = 2, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Title = "Quiz 2", Description = "Desc", CreatorId = 4, CreatedAt = DateTime.UtcNow }
        };
        var paginated = new Paginated<QuizDbo>(quizzes, 2, new PaginationOptions(), null!);
        _quizRepositoryMock.Setup(r => r.Search(It.IsAny<Quiz.Filter>(), It.IsAny<PaginationOptions>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.Search(AdminUser);

        // Assert
        Assert.That(result.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Search_AsTeacher_ShouldFilterToOwn()
    {
        // Arrange
        var quizzes = new List<QuizDbo>
        {
            new() { Id = 1, Title = "My Quiz", Description = "Desc", CreatorId = 2, CreatedAt = DateTime.UtcNow }
        };
        var paginated = new Paginated<QuizDbo>(quizzes, 1, new PaginationOptions(), null!);
        _quizRepositoryMock.Setup(r => r.Search(It.Is<Quiz.Filter>(f => f.CreatorId == 2), It.IsAny<PaginationOptions>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.Search(TeacherUser);

        // Assert
        _quizRepositoryMock.Verify(r => r.Search(It.Is<Quiz.Filter>(f => f.CreatorId == 2), It.IsAny<PaginationOptions>()), Times.Once);
    }

    [Test]
    public void Search_AsStudent_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Search(StudentUser));
        Assert.That(ex.Message, Does.Contain("Only Admin or Teacher"));
    }

    #endregion
}

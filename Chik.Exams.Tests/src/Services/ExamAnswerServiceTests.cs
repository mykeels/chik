namespace Chik.Exams.Tests.Services;

[TestFixture]
public class ExamAnswerServiceTests
{
    private Mock<IExamAnswerRepository> _answerRepositoryMock = null!;
    private Mock<IExamRepository> _examRepositoryMock = null!;
    private Mock<IAuditLogService> _auditLogServiceMock = null!;
    private Mock<ILogger<ExamAnswerService>> _loggerMock = null!;
    private ExamAnswerService _service = null!;

    private User AdminUser => new(1, "admin", [UserRole.Admin], DateTime.UtcNow, null);
    private User TeacherUser => new(2, "teacher", [UserRole.Teacher], DateTime.UtcNow, null);
    private User StudentUser => new(3, "student", [UserRole.Student], DateTime.UtcNow, null);
    private User OtherStudent => new(5, "other_student", [UserRole.Student], DateTime.UtcNow, null);

    private ExamDbo ActiveExam => new() { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = DateTime.UtcNow.AddMinutes(-10), EndedAt = null, CreatedAt = DateTime.UtcNow };
    private ExamDbo SubmittedExam => new() { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = DateTime.UtcNow.AddMinutes(-20), EndedAt = DateTime.UtcNow.AddMinutes(-5), CreatedAt = DateTime.UtcNow };
    private ExamDbo NotStartedExam => new() { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = null, EndedAt = null, CreatedAt = DateTime.UtcNow };
    private ExamDbo OtherStudentExam => new() { Id = 100, UserId = 5, QuizId = 10, CreatorId = 2, StartedAt = DateTime.UtcNow.AddMinutes(-10), EndedAt = null, CreatedAt = DateTime.UtcNow };

    [SetUp]
    public void SetUp()
    {
        _answerRepositoryMock = new Mock<IExamAnswerRepository>();
        _examRepositoryMock = new Mock<IExamRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _loggerMock = new Mock<ILogger<ExamAnswerService>>();
        _service = new ExamAnswerService(
            _answerRepositoryMock.Object,
            _examRepositoryMock.Object,
            _auditLogServiceMock.Object,
            _loggerMock.Object);
    }

    #region SubmitAnswer Tests

    [Test]
    public async Task SubmitAnswer_AsAssignedStudent_ActiveExam_ShouldSucceed()
    {
        // Arrange
        var answerDbo = new ExamAnswerDbo { Id = 1, ExamId = 100, QuestionId = 1, Answer = "A", CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(ActiveExam);
        _answerRepositoryMock.Setup(r => r.SubmitAnswer(100, 1, "A")).ReturnsAsync(answerDbo);

        // Act
        var result = await _service.SubmitAnswer(StudentUser, 100, 1, "A");

        // Assert
        Assert.That(result.Answer, Is.EqualTo("A"));
        _auditLogServiceMock.Verify(a => a.Create(It.IsAny<User>(), It.IsAny<AuditLog.Create<object>>()), Times.Once);
    }

    [Test]
    public void SubmitAnswer_AsOtherStudent_ShouldThrow()
    {
        // Arrange
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(ActiveExam);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.SubmitAnswer(OtherStudent, 100, 1, "A"));
        Assert.That(ex.Message, Does.Contain("Only the assigned student"));
    }

    [Test]
    public void SubmitAnswer_ExamNotStarted_ShouldThrow()
    {
        // Arrange
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(NotStartedExam);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.SubmitAnswer(StudentUser, 100, 1, "A"));
        Assert.That(ex.Message, Does.Contain("not been started"));
    }

    [Test]
    public void SubmitAnswer_ExamAlreadySubmitted_ShouldThrow()
    {
        // Arrange
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(SubmittedExam);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.SubmitAnswer(StudentUser, 100, 1, "A"));
        Assert.That(ex.Message, Does.Contain("already been submitted"));
    }

    #endregion

    #region Get Tests

    [Test]
    public async Task Get_AsAdmin_ShouldSucceed()
    {
        // Arrange
        var answerDbo = new ExamAnswerDbo { Id = 1, ExamId = 100, QuestionId = 1, Answer = "A", CreatedAt = DateTime.UtcNow };
        _answerRepositoryMock.Setup(r => r.Get(1)).ReturnsAsync(answerDbo);
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(ActiveExam);

        // Act
        var result = await _service.Get(AdminUser, 1);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task Get_AsStudent_OwnExam_ShouldSucceed()
    {
        // Arrange
        var answerDbo = new ExamAnswerDbo { Id = 1, ExamId = 100, QuestionId = 1, Answer = "A", CreatedAt = DateTime.UtcNow };
        _answerRepositoryMock.Setup(r => r.Get(1)).ReturnsAsync(answerDbo);
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(ActiveExam);

        // Act
        var result = await _service.Get(StudentUser, 1);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void Get_AsStudent_OtherStudentExam_ShouldThrow()
    {
        // Arrange
        var answerDbo = new ExamAnswerDbo { Id = 1, ExamId = 100, QuestionId = 1, Answer = "A", CreatedAt = DateTime.UtcNow };
        _answerRepositoryMock.Setup(r => r.Get(1)).ReturnsAsync(answerDbo);
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(OtherStudentExam);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Get(StudentUser, 1));
        Assert.That(ex.Message, Does.Contain("only view your own exam answers"));
    }

    #endregion

    #region GetByExamId Tests

    [Test]
    public async Task GetByExamId_AsStudent_OwnExam_ShouldSucceed()
    {
        // Arrange
        var answers = new List<ExamAnswerDbo>
        {
            new() { Id = 1, ExamId = 100, QuestionId = 1, Answer = "A", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, ExamId = 100, QuestionId = 2, Answer = "B", CreatedAt = DateTime.UtcNow }
        };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(ActiveExam);
        _answerRepositoryMock.Setup(r => r.GetByExamId(100)).ReturnsAsync(answers);

        // Act
        var result = await _service.GetByExamId(StudentUser, 100);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetByExamId_AsTeacher_CreatedExam_ShouldSucceed()
    {
        // Arrange
        var answers = new List<ExamAnswerDbo>
        {
            new() { Id = 1, ExamId = 100, QuestionId = 1, Answer = "A", CreatedAt = DateTime.UtcNow }
        };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(ActiveExam);
        _answerRepositoryMock.Setup(r => r.GetByExamId(100)).ReturnsAsync(answers);

        // Act
        var result = await _service.GetByExamId(TeacherUser, 100);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
    }

    #endregion

    #region Update Tests

    [Test]
    public async Task Update_AsStudent_OwnExam_NotSubmitted_ShouldSucceed()
    {
        // Arrange
        var existingAnswer = new ExamAnswerDbo { Id = 1, ExamId = 100, QuestionId = 1, Answer = "A", CreatedAt = DateTime.UtcNow };
        var updateAnswer = new ExamAnswer.Update(1, Answer: "B");
        var updatedDbo = new ExamAnswerDbo { Id = 1, ExamId = 100, QuestionId = 1, Answer = "B", CreatedAt = DateTime.UtcNow };
        
        _answerRepositoryMock.Setup(r => r.Get(1)).ReturnsAsync(existingAnswer);
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(ActiveExam);
        _answerRepositoryMock.Setup(r => r.Update(1, updateAnswer)).ReturnsAsync(updatedDbo);

        // Act
        var result = await _service.Update(StudentUser, updateAnswer);

        // Assert
        Assert.That(result.Answer, Is.EqualTo("B"));
    }

    [Test]
    public void Update_AsOtherStudent_ShouldThrow()
    {
        // Arrange
        var existingAnswer = new ExamAnswerDbo { Id = 1, ExamId = 100, QuestionId = 1, Answer = "A", CreatedAt = DateTime.UtcNow };
        var updateAnswer = new ExamAnswer.Update(1, Answer: "B");
        
        _answerRepositoryMock.Setup(r => r.Get(1)).ReturnsAsync(existingAnswer);
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(ActiveExam);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Update(OtherStudent, updateAnswer));
        Assert.That(ex.Message, Does.Contain("Only the assigned student"));
    }

    [Test]
    public void Update_AfterSubmission_ShouldThrow()
    {
        // Arrange
        var existingAnswer = new ExamAnswerDbo { Id = 1, ExamId = 100, QuestionId = 1, Answer = "A", CreatedAt = DateTime.UtcNow };
        var updateAnswer = new ExamAnswer.Update(1, Answer: "B");
        
        _answerRepositoryMock.Setup(r => r.Get(1)).ReturnsAsync(existingAnswer);
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(SubmittedExam);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.Update(StudentUser, updateAnswer));
        Assert.That(ex.Message, Does.Contain("Cannot update answers after exam submission"));
    }

    #endregion

    #region ExaminerScore Tests

    [Test]
    public async Task ExaminerScore_AsTeacher_AfterSubmission_ShouldSucceed()
    {
        // Arrange
        var existingAnswer = new ExamAnswerDbo { Id = 1, ExamId = 100, QuestionId = 1, Answer = "Good answer", CreatedAt = DateTime.UtcNow };
        var scoredDbo = new ExamAnswerDbo { Id = 1, ExamId = 100, QuestionId = 1, Answer = "Good answer", ExaminerScore = 8, ExaminerId = 2, ExaminerComment = "Well done", CreatedAt = DateTime.UtcNow };
        
        _answerRepositoryMock.Setup(r => r.Get(1)).ReturnsAsync(existingAnswer);
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(SubmittedExam);
        _answerRepositoryMock.Setup(r => r.ExaminerScore(1, 8, 2, "Well done")).ReturnsAsync(scoredDbo);

        // Act
        var result = await _service.ExaminerScore(TeacherUser, 1, 8, "Well done");

        // Assert
        Assert.That(result.ExaminerScore, Is.EqualTo(8));
    }

    [Test]
    public void ExaminerScore_AsStudent_ShouldThrow()
    {
        // Arrange
        var existingAnswer = new ExamAnswerDbo { Id = 1, ExamId = 100, QuestionId = 1, Answer = "Answer", CreatedAt = DateTime.UtcNow };
        _answerRepositoryMock.Setup(r => r.Get(1)).ReturnsAsync(existingAnswer);
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(SubmittedExam);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.ExaminerScore(StudentUser, 1, 8));
        Assert.That(ex.Message, Does.Contain("Only Admin or Teacher"));
    }

    [Test]
    public void ExaminerScore_BeforeSubmission_ShouldThrow()
    {
        // Arrange
        var existingAnswer = new ExamAnswerDbo { Id = 1, ExamId = 100, QuestionId = 1, Answer = "Answer", CreatedAt = DateTime.UtcNow };
        _answerRepositoryMock.Setup(r => r.Get(1)).ReturnsAsync(existingAnswer);
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(ActiveExam);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.ExaminerScore(TeacherUser, 1, 8));
        Assert.That(ex.Message, Does.Contain("not been submitted"));
    }

    [Test]
    public void ExaminerScore_AsTeacher_NotCreatorOrExaminer_ShouldThrow()
    {
        // Arrange
        var otherExam = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 4, StartedAt = DateTime.UtcNow.AddMinutes(-20), EndedAt = DateTime.UtcNow.AddMinutes(-5), CreatedAt = DateTime.UtcNow };
        var existingAnswer = new ExamAnswerDbo { Id = 1, ExamId = 100, QuestionId = 1, Answer = "Answer", CreatedAt = DateTime.UtcNow };
        _answerRepositoryMock.Setup(r => r.Get(1)).ReturnsAsync(existingAnswer);
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(otherExam);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.ExaminerScore(TeacherUser, 1, 8));
        Assert.That(ex.Message, Does.Contain("only score answers for exams you created"));
    }

    #endregion

    #region Search Tests

    [Test]
    public async Task Search_AsAdmin_ShouldSucceed()
    {
        // Arrange
        var answers = new List<ExamAnswerDbo>
        {
            new() { Id = 1, ExamId = 100, QuestionId = 1, Answer = "A", CreatedAt = DateTime.UtcNow }
        };
        var paginated = new Paginated<ExamAnswerDbo>(answers, 1, new PaginationOptions(), null!);
        _answerRepositoryMock.Setup(r => r.Search(It.IsAny<ExamAnswer.Filter>(), It.IsAny<PaginationOptions>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.Search(AdminUser);

        // Assert
        Assert.That(result.Items.Count, Is.EqualTo(1));
    }

    [Test]
    public void Search_AsStudent_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Search(StudentUser));
        Assert.That(ex.Message, Does.Contain("Students should use GetByExamId"));
    }

    #endregion
}

namespace Chik.Exams.Tests.Services;

[TestFixture]
public class ExamServiceTests
{
    private Mock<IExamRepository> _examRepositoryMock = null!;
    private Mock<IExamAnswerRepository> _answerRepositoryMock = null!;
    private Mock<IQuizRepository> _quizRepositoryMock = null!;
    private Mock<IQuizQuestionRepository> _questionRepositoryMock = null!;
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private Mock<IClassRepository> _classRepositoryMock = null!;
    private Mock<IAuditLogService> _auditLogServiceMock = null!;
    private Mock<ILogger<ExamService>> _loggerMock = null!;
    private ExamService _service = null!;

    private User AdminUser => new(1, "admin", [UserRole.Admin], DateTime.UtcNow, null);
    private User TeacherUser => new(2, "teacher", [UserRole.Teacher], DateTime.UtcNow, null);
    private User StudentUser => new(3, "student", [UserRole.Student], DateTime.UtcNow, null);
    private User OtherStudent => new(5, "other_student", [UserRole.Student], DateTime.UtcNow, null);

    [SetUp]
    public void SetUp()
    {
        _examRepositoryMock = new Mock<IExamRepository>();
        _answerRepositoryMock = new Mock<IExamAnswerRepository>();
        _quizRepositoryMock = new Mock<IQuizRepository>();
        _questionRepositoryMock = new Mock<IQuizQuestionRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _classRepositoryMock = new Mock<IClassRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _loggerMock = new Mock<ILogger<ExamService>>();
        _service = new ExamService(
            _examRepositoryMock.Object,
            _answerRepositoryMock.Object,
            _quizRepositoryMock.Object,
            _questionRepositoryMock.Object,
            _userRepositoryMock.Object,
            _classRepositoryMock.Object,
            _auditLogServiceMock.Object,
            _loggerMock.Object);
    }

    #region Create Tests

    [Test]
    public async Task Create_AsAdmin_ShouldSucceed()
    {
        // Arrange
        var createExam = new Exam.Create(3, 10, 1, 1); // Student 3 takes Quiz 10, created by Admin 1
        var createdDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 1, StudentClassId = 1, CreatedAt = DateTime.UtcNow };
        _userRepositoryMock.Setup(r => r.Get(3L)).ReturnsAsync(new UserDbo { Id = 3, Username = "student", Roles = (int)UserRole.Student, CreatedAt = DateTime.UtcNow });
        _userRepositoryMock.Setup(r => r.GetStudentClassIdForUser(3L)).ReturnsAsync(1);
        _examRepositoryMock.Setup(r => r.Create(It.IsAny<Exam.Create>())).ReturnsAsync(createdDbo);

        // Act
        var result = await _service.Create(AdminUser, createExam);

        // Assert
        Assert.That(result.UserId, Is.EqualTo(3));
        Assert.That(result.QuizId, Is.EqualTo(10));
    }

    [Test]
    public async Task Create_AsTeacher_ShouldSucceed()
    {
        // Arrange
        var createExam = new Exam.Create(3, 10, 2, 0);
        var createdDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StudentClassId = 1, CreatedAt = DateTime.UtcNow };
        _userRepositoryMock.Setup(r => r.Get(3L)).ReturnsAsync(new UserDbo { Id = 3, Username = "student", Roles = (int)UserRole.Student, CreatedAt = DateTime.UtcNow });
        _userRepositoryMock.Setup(r => r.GetStudentClassIdForUser(3L)).ReturnsAsync(1);
        _classRepositoryMock.Setup(r => r.GetClassIdsForTeacher(2L)).ReturnsAsync(new List<int> { 1 });
        _examRepositoryMock.Setup(r => r.Create(It.IsAny<Exam.Create>())).ReturnsAsync(createdDbo);

        // Act
        var result = await _service.Create(TeacherUser, createExam);

        // Assert
        Assert.That(result.CreatorId, Is.EqualTo(2));
    }

    [Test]
    public void Create_AsStudent_ShouldThrow()
    {
        // Arrange
        var createExam = new Exam.Create(3, 10, 3, 0);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Create(StudentUser, createExam));
        Assert.That(ex.Message, Does.Contain("Only Admin or Teacher"));
    }

    #endregion

    #region Get Tests

    [Test]
    public async Task Get_AsAdmin_AnyExam_ShouldSucceed()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);

        // Act
        var result = await _service.Get(AdminUser, 100);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task Get_AsTeacher_OwnCreatedExam_ShouldSucceed()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);

        // Act
        var result = await _service.Get(TeacherUser, 100);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task Get_AsStudent_OwnExam_ShouldSucceed()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);

        // Act
        var result = await _service.Get(StudentUser, 100);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void Get_AsStudent_OtherStudentExam_ShouldThrow()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 5, QuizId = 10, CreatorId = 2, CreatedAt = DateTime.UtcNow }; // Other student's exam
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Get(StudentUser, 100));
        Assert.That(ex.Message, Does.Contain("only view your own exams"));
    }

    #endregion

    #region Start Tests

    [Test]
    public async Task Start_AsAssignedStudent_NotStarted_ShouldSucceed()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = null, CreatedAt = DateTime.UtcNow };
        var startedDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);
        _examRepositoryMock.Setup(r => r.Start(100)).ReturnsAsync(startedDbo);

        // Act
        var result = await _service.Start(StudentUser, 100);

        // Assert
        Assert.That(result.StartedAt, Is.Not.Null);
    }

    [Test]
    public void Start_AsOtherStudent_ShouldThrow()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 5, QuizId = 10, CreatorId = 2, StartedAt = null, CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Start(StudentUser, 100));
        Assert.That(ex.Message, Does.Contain("Only the assigned student"));
    }

    [Test]
    public void Start_AlreadyStarted_ShouldThrow()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = DateTime.UtcNow.AddMinutes(-10), CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.Start(StudentUser, 100));
        Assert.That(ex.Message, Does.Contain("already been started"));
    }

    #endregion

    #region End Tests

    [Test]
    public async Task End_AsAssignedStudent_Started_ShouldSucceed()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = DateTime.UtcNow.AddMinutes(-10), EndedAt = null, CreatedAt = DateTime.UtcNow };
        var endedDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = DateTime.UtcNow.AddMinutes(-10), EndedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);
        _examRepositoryMock.Setup(r => r.End(100)).ReturnsAsync(endedDbo);

        // Act
        var result = await _service.End(StudentUser, 100);

        // Assert
        Assert.That(result.EndedAt, Is.Not.Null);
    }

    [Test]
    public void End_NotStarted_ShouldThrow()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = null, CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.End(StudentUser, 100));
        Assert.That(ex.Message, Does.Contain("not been started"));
    }

    [Test]
    public void End_AlreadyEnded_ShouldThrow()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = DateTime.UtcNow.AddMinutes(-20), EndedAt = DateTime.UtcNow.AddMinutes(-5), CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.End(StudentUser, 100));
        Assert.That(ex.Message, Does.Contain("already been submitted"));
    }

    #endregion

    #region Mark Tests

    [Test]
    public async Task Mark_AsTeacher_CreatedExam_AfterSubmission_ShouldSucceed()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = DateTime.UtcNow.AddMinutes(-20), EndedAt = DateTime.UtcNow.AddMinutes(-5), CreatedAt = DateTime.UtcNow };
        var markedDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = DateTime.UtcNow.AddMinutes(-20), EndedAt = DateTime.UtcNow.AddMinutes(-5), Score = 85, ExaminerId = 2, CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);
        _examRepositoryMock.Setup(r => r.Mark(100, 85, 2, "Good job!")).ReturnsAsync(markedDbo);

        // Act
        var result = await _service.Mark(TeacherUser, 100, 85, "Good job!");

        // Assert
        Assert.That(result.Score, Is.EqualTo(85));
    }

    [Test]
    public void Mark_BeforeSubmission_ShouldThrow()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = DateTime.UtcNow.AddMinutes(-10), EndedAt = null, CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.Mark(TeacherUser, 100, 85));
        Assert.That(ex.Message, Does.Contain("not been submitted"));
    }

    [Test]
    public void Mark_AsStudent_ShouldThrow()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = DateTime.UtcNow.AddMinutes(-20), EndedAt = DateTime.UtcNow.AddMinutes(-5), CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Mark(StudentUser, 100, 85));
        Assert.That(ex.Message, Does.Contain("Only Admin or Teacher"));
    }

    #endregion

    #region GetScores Tests

    [Test]
    public async Task GetScores_AsStudent_OwnExam_ShouldSucceed()
    {
        // Arrange
        var examDbo = new ExamDbo 
        { 
            Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, 
            StartedAt = DateTime.UtcNow.AddMinutes(-20), 
            EndedAt = DateTime.UtcNow.AddMinutes(-5), 
            CreatedAt = DateTime.UtcNow,
            Answers = new List<ExamAnswerDbo>
            {
                new() { Id = 1, ExamId = 100, QuestionId = 1, Answer = "A", AutoScore = 10 },
                new() { Id = 2, ExamId = 100, QuestionId = 2, Answer = "B", AutoScore = 5, ExaminerScore = 8 }
            }
        };
        var questions = new List<QuizQuestionDbo>
        {
            new() { Id = 1, QuizId = 10, Score = 10, Prompt = "Q1", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, QuizId = 10, Score = 10, Prompt = "Q2", CreatedAt = DateTime.UtcNow }
        };
        _examRepositoryMock.Setup(r => r.Get(100, true)).ReturnsAsync(examDbo);
        _questionRepositoryMock.Setup(r => r.GetByQuizId(10, false)).ReturnsAsync(questions);

        // Act
        var result = await _service.GetScores(StudentUser, 100);

        // Assert
        Assert.That(result.ExamId, Is.EqualTo(100));
        Assert.That(result.TotalScore, Is.EqualTo(18)); // 10 (auto) + 8 (examiner override)
        Assert.That(result.MaxPossibleScore, Is.EqualTo(20));
        Assert.That(result.AnswerScores.Count, Is.EqualTo(2));
    }

    [Test]
    public void GetScores_AsStudent_OtherExam_ShouldThrow()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 5, QuizId = 10, CreatorId = 2, CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, true)).ReturnsAsync(examDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.GetScores(StudentUser, 100));
        Assert.That(ex.Message, Does.Contain("only view your own exam scores"));
    }

    #endregion

    #region GetPendingExams Tests

    [Test]
    public async Task GetPendingExams_AsStudent_OwnExams_ShouldSucceed()
    {
        // Arrange
        var exams = new List<ExamDbo>
        {
            new() { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, StartedAt = null, CreatedAt = DateTime.UtcNow }
        };
        var paginated = new Paginated<ExamDbo>(exams, 1, new PaginationOptions(), null!);
        _examRepositoryMock.Setup(r => r.Search(It.Is<Exam.Filter>(f => f.UserId == 3 && f.IsStarted == false), It.IsAny<PaginationOptions>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.GetPendingExams(StudentUser);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
    }

    [Test]
    public void GetPendingExams_AsStudent_OtherStudent_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.GetPendingExams(StudentUser, 5)); // Trying to view student 5's exams
        Assert.That(ex.Message, Does.Contain("only view your own pending exams"));
    }

    #endregion

    #region Cancel Tests

    [Test]
    public async Task Cancel_AsTeacher_OwnCreatedExam_ShouldSucceed()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, CreatedAt = DateTime.UtcNow };
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);
        _examRepositoryMock.Setup(r => r.Delete(100)).Returns(Task.CompletedTask);

        // Act
        await _service.Cancel(TeacherUser, 100);

        // Assert
        _examRepositoryMock.Verify(r => r.Delete(100), Times.Once);
    }

    [Test]
    public void Cancel_AsTeacher_OtherCreatedExam_ShouldThrow()
    {
        // Arrange
        var examDbo = new ExamDbo { Id = 100, UserId = 3, QuizId = 10, CreatorId = 4, CreatedAt = DateTime.UtcNow }; // Created by teacher 4
        _examRepositoryMock.Setup(r => r.Get(100, false)).ReturnsAsync(examDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Cancel(TeacherUser, 100));
        Assert.That(ex.Message, Does.Contain("only cancel exams you created"));
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_AsAdmin_ShouldSucceed()
    {
        // Arrange
        _examRepositoryMock.Setup(r => r.Delete(100)).Returns(Task.CompletedTask);

        // Act
        await _service.Delete(AdminUser, 100);

        // Assert
        _examRepositoryMock.Verify(r => r.Delete(100), Times.Once);
    }

    [Test]
    public void Delete_AsNonAdmin_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Delete(TeacherUser, 100));
        Assert.That(ex.Message, Does.Contain("Only Admin can delete"));
    }

    #endregion

    #region Search Tests

    [Test]
    public async Task Search_AsAdmin_ShouldReturnAll()
    {
        // Arrange
        var exams = new List<ExamDbo>
        {
            new() { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, CreatedAt = DateTime.UtcNow },
            new() { Id = 101, UserId = 5, QuizId = 11, CreatorId = 4, CreatedAt = DateTime.UtcNow }
        };
        var paginated = new Paginated<ExamDbo>(exams, 2, new PaginationOptions(), null!);
        _examRepositoryMock.Setup(r => r.Search(It.IsAny<Exam.Filter>(), It.IsAny<PaginationOptions>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.Search(AdminUser);

        // Assert
        Assert.That(result.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Search_AsStudent_ShouldFilterToOwn()
    {
        // Arrange
        var exams = new List<ExamDbo>
        {
            new() { Id = 100, UserId = 3, QuizId = 10, CreatorId = 2, CreatedAt = DateTime.UtcNow }
        };
        var paginated = new Paginated<ExamDbo>(exams, 1, new PaginationOptions(), null!);
        _examRepositoryMock.Setup(r => r.Search(It.Is<Exam.Filter>(f => f.UserId == 3), It.IsAny<PaginationOptions>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.Search(StudentUser);

        // Assert
        _examRepositoryMock.Verify(r => r.Search(It.Is<Exam.Filter>(f => f.UserId == 3), It.IsAny<PaginationOptions>()), Times.Once);
    }

    #endregion
}

namespace Chik.Exams.Tests.Services;

[TestFixture]
public class AuditLogServiceTests
{
    private Mock<IAuditLogRepository> _auditLogRepositoryMock = null!;
    private Mock<ILogger<AuditLogService>> _loggerMock = null!;
    private AuditLogService _service = null!;

    private User AdminUser => new(1, "admin", [UserRole.Admin], DateTime.UtcNow, null);
    private User TeacherUser => new(2, "teacher", [UserRole.Teacher], DateTime.UtcNow, null);
    private User StudentUser => new(3, "student", [UserRole.Student], DateTime.UtcNow, null);

    [SetUp]
    public void SetUp()
    {
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _loggerMock = new Mock<ILogger<AuditLogService>>();
        _service = new AuditLogService(_auditLogRepositoryMock.Object, _loggerMock.Object);
    }

    #region Create Tests

    [Test]
    public async Task Create_ShouldSerializePropertiesAndCreateLog()
    {
        // Arrange
        var auditLog = new AuditLog.Create<User.Create>("UserService.Create", 10, new User.Create("newuser", "password", [UserRole.Student]));
        var createdDbo = new AuditLogDbo { Id = 1, UserId = 1, Service = "UserService.Create", EntityId = 10, Properties = "{}", CreatedAt = DateTime.UtcNow };
        _auditLogRepositoryMock.Setup(r => r.Create(1, It.IsAny<AuditLog.Create>())).ReturnsAsync(createdDbo);

        // Act
        var result = await _service.Create(AdminUser, auditLog);

        // Assert
        Assert.That(result.Service, Is.EqualTo("UserService.Create"));
        _auditLogRepositoryMock.Verify(r => r.Create(1, It.Is<AuditLog.Create>(c => 
            c.Service == "UserService.Create" && 
            c.EntityId == 10 && 
            c.Properties.Contains("newuser"))), Times.Once);
    }

    #endregion

    #region Get Tests

    [Test]
    public async Task Get_AsAdmin_ShouldSucceed()
    {
        // Arrange
        var auditLogDbo = new AuditLogDbo { Id = 1, UserId = 1, Service = "Test", EntityId = 10, Properties = "{}", CreatedAt = DateTime.UtcNow };
        _auditLogRepositoryMock.Setup(r => r.Get(1)).ReturnsAsync(auditLogDbo);

        // Act
        var result = await _service.Get(AdminUser, 1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Service, Is.EqualTo("Test"));
    }

    [Test]
    public void Get_AsNonAdmin_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Get(TeacherUser, 1));
        Assert.That(ex.Message, Does.Contain("Only Admin can view audit logs"));
    }

    [Test]
    public void Get_AsStudent_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Get(StudentUser, 1));
        Assert.That(ex.Message, Does.Contain("Only Admin can view audit logs"));
    }

    #endregion

    #region GetByService Tests

    [Test]
    public async Task GetByService_AsAdmin_ShouldSucceed()
    {
        // Arrange
        var auditLogs = new List<AuditLogDbo>
        {
            new() { Id = 1, UserId = 1, Service = "UserService.Create", EntityId = 10, Properties = "{}", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, UserId = 1, Service = "UserService.Create", EntityId = 10, Properties = "{}", CreatedAt = DateTime.UtcNow.AddMinutes(-5) }
        };
        _auditLogRepositoryMock.Setup(r => r.GetByService("UserService.Create", 10)).ReturnsAsync(auditLogs);

        // Act
        var result = await _service.GetByService(AdminUser, "UserService.Create", 10);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public void GetByService_AsNonAdmin_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.GetByService(TeacherUser, "UserService.Create", 10));
        Assert.That(ex.Message, Does.Contain("Only Admin can view audit logs"));
    }

    #endregion

    #region GetByUserId Tests

    [Test]
    public async Task GetByUserId_AsAdmin_ShouldSucceed()
    {
        // Arrange
        var auditLogs = new List<AuditLogDbo>
        {
            new() { Id = 1, UserId = 2, Service = "QuizService.Create", EntityId = 10, Properties = "{}", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, UserId = 2, Service = "ExamService.Create", EntityId = 20, Properties = "{}", CreatedAt = DateTime.UtcNow.AddMinutes(-5) }
        };
        _auditLogRepositoryMock.Setup(r => r.GetByUserId(2)).ReturnsAsync(auditLogs);

        // Act
        var result = await _service.GetByUserId(AdminUser, 2);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public void GetByUserId_AsNonAdmin_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.GetByUserId(StudentUser, 2));
        Assert.That(ex.Message, Does.Contain("Only Admin can view audit logs"));
    }

    #endregion

    #region Search Tests

    [Test]
    public async Task Search_AsAdmin_ShouldSucceed()
    {
        // Arrange
        var auditLogs = new List<AuditLogDbo>
        {
            new() { Id = 1, UserId = 1, Service = "Test", EntityId = 10, Properties = "{}", CreatedAt = DateTime.UtcNow }
        };
        var paginated = new Paginated<AuditLogDbo>(auditLogs, 1, new PaginationOptions(), null!);
        _auditLogRepositoryMock.Setup(r => r.Search(It.IsAny<AuditLog.Filter>(), It.IsAny<PaginationOptions>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.Search(AdminUser);

        // Assert
        Assert.That(result.Items.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Search_AsAdmin_WithFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var filter = new AuditLog.Filter(UserId: 2, Service: "QuizService.Create");
        var auditLogs = new List<AuditLogDbo>();
        var paginated = new Paginated<AuditLogDbo>(auditLogs, 0, new PaginationOptions(), null!);
        _auditLogRepositoryMock.Setup(r => r.Search(filter, It.IsAny<PaginationOptions>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.Search(AdminUser, filter);

        // Assert
        _auditLogRepositoryMock.Verify(r => r.Search(filter, It.IsAny<PaginationOptions>()), Times.Once);
    }

    [Test]
    public void Search_AsTeacher_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Search(TeacherUser));
        Assert.That(ex.Message, Does.Contain("Only Admin can search audit logs"));
    }

    [Test]
    public void Search_AsStudent_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Search(StudentUser));
        Assert.That(ex.Message, Does.Contain("Only Admin can search audit logs"));
    }

    #endregion
}

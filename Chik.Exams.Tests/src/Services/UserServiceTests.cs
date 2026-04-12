namespace Chik.Exams.Tests.Services;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private Mock<IClassRepository> _classRepositoryMock = null!;
    private Mock<IAuditLogService> _auditLogServiceMock = null!;
    private Mock<ILogger<UserService>> _loggerMock = null!;
    private UserService _service = null!;

    private User AdminUser => new(1, "admin", [UserRole.Admin], DateTime.UtcNow, null);
    private User TeacherUser => new(2, "teacher", [UserRole.Teacher], DateTime.UtcNow, null);
    private User StudentUser => new(3, "student", [UserRole.Student], DateTime.UtcNow, null);

    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _classRepositoryMock = new Mock<IClassRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _service = new UserService(_userRepositoryMock.Object, _classRepositoryMock.Object, _auditLogServiceMock.Object, _loggerMock.Object);
    }

    #region Create Tests

    [Test]
    public async Task Create_AsAdmin_WithAnyRole_ShouldSucceed()
    {
        // Arrange
        var createUser = new User.Create("newuser", "password123", [UserRole.Teacher]);
        var createdDbo = new UserDbo { Id = 10, Username = "newuser", Roles = UserRole.Teacher.ToInt32(), CreatedAt = DateTime.UtcNow };
        _userRepositoryMock.Setup(r => r.Create(createUser)).ReturnsAsync(createdDbo);

        // Act
        var result = await _service.Create(AdminUser, createUser);

        // Assert
        Assert.That(result.Username, Is.EqualTo("newuser"));
        _userRepositoryMock.Verify(r => r.Create(createUser), Times.Once);
        _auditLogServiceMock.Verify(a => a.Create(It.IsAny<User>(), It.IsAny<AuditLog.Create<User.Create>>()), Times.Once);
    }

    [Test]
    public async Task Create_AsTeacher_WithStudentRole_ShouldSucceed()
    {
        // Arrange
        var createUser = new User.Create("newstudent", "password123", [UserRole.Student], ClassId: 1);
        var createdDbo = new UserDbo { Id = 10, Username = "newstudent", Roles = UserRole.Student.ToInt32(), CreatedAt = DateTime.UtcNow };
        _classRepositoryMock.Setup(r => r.GetClassIdsForTeacher(2L)).ReturnsAsync(new List<int> { 1 });
        _userRepositoryMock.Setup(r => r.Create(createUser)).ReturnsAsync(createdDbo);

        // Act
        var result = await _service.Create(TeacherUser, createUser);

        // Assert
        Assert.That(result.Username, Is.EqualTo("newstudent"));
    }

    [Test]
    public void Create_AsTeacher_WithTeacherRole_ShouldThrow()
    {
        // Arrange
        var createUser = new User.Create("newteacher", "password123", [UserRole.Teacher]);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Create(TeacherUser, createUser));
        Assert.That(ex.Message, Does.Contain("Teachers can only create Student"));
    }

    [Test]
    public void Create_AsStudent_ShouldThrow()
    {
        // Arrange
        var createUser = new User.Create("newuser", "password123", [UserRole.Student]);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Create(StudentUser, createUser));
        Assert.That(ex.Message, Does.Contain("Only Admin or Teacher"));
    }

    #endregion

    #region Get Tests

    [Test]
    public async Task Get_AsAdmin_AnyUser_ShouldSucceed()
    {
        // Arrange
        var userDbo = new UserDbo { Id = 3, Username = "student", Roles = UserRole.Student.ToInt32() };
        _userRepositoryMock.Setup(r => r.Get(3L)).ReturnsAsync(userDbo);

        // Act
        var result = await _service.Get(AdminUser, 3);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo("student"));
    }

    [Test]
    public async Task Get_AsUser_OwnProfile_ShouldSucceed()
    {
        // Arrange
        var userDbo = new UserDbo { Id = 3, Username = "student", Roles = UserRole.Student.ToInt32() };
        _userRepositoryMock.Setup(r => r.Get(3L)).ReturnsAsync(userDbo);

        // Act
        var result = await _service.Get(StudentUser, 3);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void Get_AsUser_OtherProfile_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Get(StudentUser, 1));
        Assert.That(ex.Message, Does.Contain("only view your own profile"));
    }

    #endregion

    #region Update Tests

    [Test]
    public async Task Update_AsAdmin_AnyUser_ShouldSucceed()
    {
        // Arrange
        var updateUser = new User.Update(3, Username: "updatedname");
        var updatedDbo = new UserDbo { Id = 3, Username = "updatedname", Roles = UserRole.Student.ToInt32() };
        _userRepositoryMock.Setup(r => r.Update(3, updateUser)).ReturnsAsync(updatedDbo);

        // Act
        var result = await _service.Update(AdminUser, updateUser);

        // Assert
        Assert.That(result.Username, Is.EqualTo("updatedname"));
    }

    [Test]
    public async Task Update_AsUser_OwnProfile_ShouldSucceed()
    {
        // Arrange
        var updateUser = new User.Update(3, Username: "mynewname");
        var updatedDbo = new UserDbo { Id = 3, Username = "mynewname", Roles = UserRole.Student.ToInt32() };
        _userRepositoryMock.Setup(r => r.Update(3, updateUser)).ReturnsAsync(updatedDbo);

        // Act
        var result = await _service.Update(StudentUser, updateUser);

        // Assert
        Assert.That(result.Username, Is.EqualTo("mynewname"));
    }

    [Test]
    public void Update_AsUser_OtherProfile_ShouldThrow()
    {
        // Arrange
        var updateUser = new User.Update(1, Username: "hackername");

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Update(StudentUser, updateUser));
        Assert.That(ex.Message, Does.Contain("only update your own profile"));
    }

    [Test]
    public void Update_AsNonAdmin_ChangingRoles_ShouldThrow()
    {
        // Arrange
        var updateUser = new User.Update(3, Roles: [UserRole.Admin]);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Update(StudentUser, updateUser));
        Assert.That(ex.Message, Does.Contain("Only Admin can change user roles"));
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_AsAdmin_ShouldSucceed()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.Delete(3)).Returns(Task.CompletedTask);

        // Act
        await _service.Delete(AdminUser, 3);

        // Assert
        _userRepositoryMock.Verify(r => r.Delete(3), Times.Once);
    }

    [Test]
    public void Delete_AsNonAdmin_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Delete(TeacherUser, 3));
        Assert.That(ex.Message, Does.Contain("Only Admin can delete"));
    }

    #endregion

    #region ChangePassword Tests

    [Test]
    public async Task ChangePassword_OwnPassword_WithCorrectCurrent_ShouldSucceed()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("oldpassword");
        var userDbo = new UserDbo { Id = 3, Username = "student", Password = hashedPassword, Roles = UserRole.Student.ToInt32() };
        _userRepositoryMock.Setup(r => r.Get(3L)).ReturnsAsync(userDbo);
        _userRepositoryMock.Setup(r => r.Update(3, It.IsAny<User.Update>())).ReturnsAsync(userDbo);

        // Act
        await _service.ChangePassword(StudentUser, 3, "oldpassword", "newpassword");

        // Assert
        _userRepositoryMock.Verify(r => r.Update(3, It.IsAny<User.Update>()), Times.Once);
    }

    [Test]
    public void ChangePassword_OwnPassword_WithIncorrectCurrent_ShouldThrow()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var userDbo = new UserDbo { Id = 3, Username = "student", Password = hashedPassword, Roles = UserRole.Student.ToInt32() };
        _userRepositoryMock.Setup(r => r.Get(3L)).ReturnsAsync(userDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.ChangePassword(StudentUser, 3, "wrongpassword", "newpassword"));
        Assert.That(ex.Message, Does.Contain("Current password is incorrect"));
    }

    [Test]
    public void ChangePassword_OtherUserPassword_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.ChangePassword(StudentUser, 1, "anypassword", "newpassword"));
        Assert.That(ex.Message, Does.Contain("only change your own password"));
    }

    #endregion

    #region Search Tests

    [Test]
    public async Task Search_AsAdmin_ShouldReturnAll()
    {
        // Arrange
        var users = new List<UserDbo>
        {
            new() { Id = 1, Username = "admin", Roles = UserRole.Admin.ToInt32() },
            new() { Id = 2, Username = "teacher", Roles = UserRole.Teacher.ToInt32() }
        };
        var paginated = new Paginated<UserDbo>(users, 2, new PaginationOptions(), null!);
        _userRepositoryMock.Setup(r => r.Search(It.IsAny<User.Filter>(), It.IsAny<PaginationOptions>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.Search(AdminUser);

        // Assert
        Assert.That(result.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Search_AsTeacher_ShouldFilterToStudents()
    {
        // Arrange
        var users = new List<UserDbo>
        {
            new() { Id = 3, Username = "student", Roles = UserRole.Student.ToInt32() }
        };
        var paginated = new Paginated<UserDbo>(users, 1, new PaginationOptions(), null!);
        _userRepositoryMock.Setup(r => r.Search(It.Is<User.Filter>(f => f.Roles != null), It.IsAny<PaginationOptions>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.Search(TeacherUser);

        // Assert
        _userRepositoryMock.Verify(r => r.Search(It.Is<User.Filter>(f => f.Roles != null), It.IsAny<PaginationOptions>()), Times.Once);
    }

    [Test]
    public async Task Search_AsStudent_ShouldFilterToSelf()
    {
        // Arrange
        var users = new List<UserDbo>
        {
            new() { Id = 3, Username = "student", Roles = UserRole.Student.ToInt32() }
        };
        var paginated = new Paginated<UserDbo>(users, 1, new PaginationOptions(), null!);
        _userRepositoryMock.Setup(r => r.Search(It.Is<User.Filter>(f => f.UserIds != null && f.UserIds.Contains(3)), It.IsAny<PaginationOptions>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.Search(StudentUser);

        // Assert
        _userRepositoryMock.Verify(r => r.Search(It.Is<User.Filter>(f => f.UserIds != null && f.UserIds.Contains(3)), It.IsAny<PaginationOptions>()), Times.Once);
    }

    #endregion
}

public static class UserRoleTestExtensions
{
    public static int ToInt32(this UserRole role) => (int)role;
}


using Chik.Exams.Logins.Repositories;

namespace Chik.Exams.Tests.Services;

[TestFixture]
public class LoginServiceTests
{
    private Mock<IJwtService> _jwtServiceMock = null!;
    private Mock<ILoginRepository> _loginRepositoryMock = null!;
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private Mock<ILogger<LoginService>> _loggerMock = null!;
    private LoginService _service = null!;

    private User AdminUser => new(1, "admin", [UserRole.Admin], DateTime.UtcNow, null);
    private User TeacherUser => new(2, "teacher", [UserRole.Teacher], DateTime.UtcNow, null);
    private User StudentUser => new(3, "student", [UserRole.Student], DateTime.UtcNow, null);

    [SetUp]
    public void SetUp()
    {
        _jwtServiceMock = new Mock<IJwtService>();
        _loginRepositoryMock = new Mock<ILoginRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<LoginService>>();
        _service = new LoginService(
_jwtServiceMock.Object,
    TestUtils.TimeProvider,
            _loginRepositoryMock.Object, 
            _userRepositoryMock.Object, 
            _loggerMock.Object);
    }

    #region Authenticate Tests

    [Test]
    public async Task Authenticate_WithValidCredentials_ShouldReturnUser()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var userDbo = new UserDbo { Id = 3, Username = "student", Password = hashedPassword, Roles = UserRole.Student.ToInt32() };
        _userRepositoryMock.Setup(r => r.Get("student")).ReturnsAsync(userDbo);

        // Act
        var result = await _service.Authenticate("student", "correctpassword");

        // Assert
        Assert.That(result.Username, Is.EqualTo("student"));
        Assert.That(result.Id, Is.EqualTo(3));
    }

    [Test]
    public void Authenticate_WithInvalidUsername_ShouldThrow()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.Get("nonexistent")).ReturnsAsync((UserDbo?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Authenticate("nonexistent", "anypassword"));
        Assert.That(ex.Message, Does.Contain("Invalid username or password"));
    }

    [Test]
    public void Authenticate_WithInvalidPassword_ShouldThrow()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var userDbo = new UserDbo { Id = 3, Username = "student", Password = hashedPassword, Roles = UserRole.Student.ToInt32() };
        _userRepositoryMock.Setup(r => r.Get("student")).ReturnsAsync(userDbo);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.Authenticate("student", "wrongpassword"));
        Assert.That(ex.Message, Does.Contain("Invalid username or password"));
    }

    [Test]
    public async Task Authenticate_WithAdminUser_ShouldReturnUserWithAdminRole()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("adminpass");
        var userDbo = new UserDbo { Id = 1, Username = "admin", Password = hashedPassword, Roles = UserRole.Admin.ToInt32() };
        _userRepositoryMock.Setup(r => r.Get("admin")).ReturnsAsync(userDbo);

        // Act
        var result = await _service.Authenticate("admin", "adminpass");

        // Assert
        Assert.That(result.IsAdmin(), Is.True);
    }

    #endregion

    #region Create Tests

    [Test]
    public async Task Create_ShouldCallRepository()
    {
        // Arrange
        var ipAddressLocationId = Guid.NewGuid();
        var loginCreate = new Login.Create(3, ipAddressLocationId);
        _loginRepositoryMock.Setup(r => r.Create(3, loginCreate)).Returns(Task.FromResult(new LoginDbo()));

        // Act
        await _service.Create(StudentUser, loginCreate);

        // Assert
        _loginRepositoryMock.Verify(r => r.Create(3, loginCreate), Times.Once);
    }

    #endregion

    #region Search Tests

    [Test]
    public async Task Search_AsAdmin_ShouldReturnAllLogins()
    {
        // Arrange
        var logins = new List<LoginDbo>
        {
            new() { Id = Guid.NewGuid(), UserId = 2, IpAddressLocationId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), UserId = 3, IpAddressLocationId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
        };
        var paginated = new Paginated<LoginDbo>(logins, 2, new PaginationOptions(), null!);
        _loginRepositoryMock.Setup(r => r.Search(It.IsAny<Login.Filter>(), It.IsAny<PaginationOptions>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.Search(AdminUser);

        // Assert
        Assert.That(result.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Search_AsNonAdmin_ShouldFilterToOwnLogins()
    {
        // Arrange
        var logins = new List<LoginDbo>
        {
            new() { Id = Guid.NewGuid(), UserId = 3, IpAddressLocationId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
        };
        var paginated = new Paginated<LoginDbo>(logins, 1, new PaginationOptions(), null!);
        _loginRepositoryMock.Setup(r => r.Search(It.Is<Login.Filter>(f => f.UserId == 3), It.IsAny<PaginationOptions>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.Search(StudentUser);

        // Assert
        _loginRepositoryMock.Verify(r => r.Search(It.Is<Login.Filter>(f => f.UserId == 3), It.IsAny<PaginationOptions>()), Times.Once);
    }

    [Test]
    public async Task Search_AsTeacher_ShouldFilterToOwnLogins()
    {
        // Arrange
        var logins = new List<LoginDbo>
        {
            new() { Id = Guid.NewGuid(), UserId = 2, IpAddressLocationId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
        };
        var paginated = new Paginated<LoginDbo>(logins, 1, new PaginationOptions(), null!);
        _loginRepositoryMock.Setup(r => r.Search(It.Is<Login.Filter>(f => f.UserId == 2), It.IsAny<PaginationOptions>()))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.Search(TeacherUser);

        // Assert
        _loginRepositoryMock.Verify(r => r.Search(It.Is<Login.Filter>(f => f.UserId == 2), It.IsAny<PaginationOptions>()), Times.Once);
    }

    [Test]
    public async Task Search_WithPagination_ShouldPassPaginationToRepository()
    {
        // Arrange
        var pagination = new PaginationOptions(2, 10);
        var logins = new List<LoginDbo>();
        var paginated = new Paginated<LoginDbo>(logins, 0, pagination, null!);
        _loginRepositoryMock.Setup(r => r.Search(It.IsAny<Login.Filter>(), pagination))
            .ReturnsAsync(paginated);

        // Act
        var result = await _service.Search(AdminUser, pagination: pagination);

        // Assert
        _loginRepositoryMock.Verify(r => r.Search(It.IsAny<Login.Filter>(), pagination), Times.Once);
    }

    #endregion
}

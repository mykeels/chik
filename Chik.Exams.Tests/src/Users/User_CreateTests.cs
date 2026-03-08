using Chik.Exams.Users.Repositories;

namespace Chik.Exams.Tests.Users;

[TestFixture]
public class User_CreateTests
{
    private UserRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        var factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<UserRepository>>();
        _repository = new UserRepository(factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var create = new User.Create("testuser", "password123", User.RolesOf(UserRole.Student));

        // Act
        var result = await _repository.Create(create);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo("testuser"));
        Assert.That(result.Roles, Is.EqualTo((int)UserRole.Student));
    }

    [Test]
    public async Task Create_WithDuplicateUsername_ShouldThrowException()
    {
        // Arrange
        await _repository.Create(new User.Create("testuser", "password123", User.RolesOf(UserRole.Student)));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository.Create(new User.Create("testuser", "password456", User.RolesOf(UserRole.Teacher))));
    }
}

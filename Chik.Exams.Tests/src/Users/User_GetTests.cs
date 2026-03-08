using Chik.Exams.Users.Repositories;

namespace Chik.Exams.Tests.Users;

[TestFixture]
public class User_GetTests
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
    public async Task Get_ById_WithExistingId_ShouldReturnUser()
    {
        // Arrange
        var created = await _repository.Create(new User.Create("testuser", "password123", (int)UserRole.Student));

        // Act
        var result = await _repository.Get(created.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(created.Id));
    }

    [Test]
    public async Task Get_ById_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.Get(99999L);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Get_ByUsername_WithExistingUsername_ShouldReturnUser()
    {
        // Arrange
        await _repository.Create(new User.Create("testuser", "password123", (int)UserRole.Student));

        // Act
        var result = await _repository.Get("testuser");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo("testuser"));
    }

    [Test]
    public async Task Get_ByUsername_WithNonExistingUsername_ShouldReturnNull()
    {
        // Act
        var result = await _repository.Get("nonexistent");

        // Assert
        Assert.That(result, Is.Null);
    }
}

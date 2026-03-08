using Chik.Exams.Users.Repositories;

namespace Chik.Exams.Tests.Users;

[TestFixture]
public class User_UpdateTests
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
    public async Task Update_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var created = await _repository.Create(new User.Create("testuser", "password123", (int)UserRole.Student));

        // Act
        var result = await _repository.Update(created.Id, new User.Update(created.Id, Username: "updateduser"));

        // Assert
        Assert.That(result.Username, Is.EqualTo("updateduser"));
    }

    [Test]
    public async Task Update_WithNonExistingId_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _repository.Update(99999, new User.Update(99999, Username: "updateduser")));
    }

    [Test]
    public async Task Update_Roles_ShouldUpdateRoles()
    {
        // Arrange
        var created = await _repository.Create(new User.Create("testuser", "password123", (int)UserRole.Student));

        // Act
        var result = await _repository.Update(created.Id, new User.Update(created.Id, Roles: (int)(UserRole.Student | UserRole.Teacher)));

        // Assert
        Assert.That(result.Roles, Is.EqualTo((int)(UserRole.Student | UserRole.Teacher)));
    }
}

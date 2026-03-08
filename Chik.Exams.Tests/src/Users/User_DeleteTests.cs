using Chik.Exams.Users.Repositories;

namespace Chik.Exams.Tests.Users;

[TestFixture]
public class User_DeleteTests
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
    public async Task Delete_WithExistingId_ShouldDeleteUser()
    {
        // Arrange
        var created = await _repository.Create(new User.Create("testuser", "password123", (int)UserRole.Student));

        // Act
        await _repository.Delete(created.Id);

        // Assert
        var result = await _repository.Get(created.Id);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Delete_WithNonExistingId_ShouldNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _repository.Delete(99999));
    }
}

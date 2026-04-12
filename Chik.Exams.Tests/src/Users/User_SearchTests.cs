
namespace Chik.Exams.Tests.Users;

[TestFixture]
public class User_SearchTests
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
    public async Task Search_WithNoFilter_ShouldReturnAllUsers()
    {
        // Arrange
        await _repository.Create(new User.Create("user1", "password", User.RolesOf(UserRole.Student)));
        await _repository.Create(new User.Create("user2", "password", User.RolesOf(UserRole.Teacher)));

        // Act
        var result = await _repository.Search();

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Search_WithRoleFilter_ShouldReturnFilteredUsers()
    {
        // Arrange
        await _repository.Create(new User.Create("student", "password", User.RolesOf(UserRole.Student)));
        await _repository.Create(new User.Create("teacher", "password", User.RolesOf(UserRole.Teacher)));

        // Act
        var result = await _repository.Search(new User.Filter(Roles: (int)UserRole.Student));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.Items[0].Username, Is.EqualTo("student"));
    }

    [Test]
    public async Task Search_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _repository.Create(new User.Create($"user{i}", "password", User.RolesOf(UserRole.Student)));
        }

        // Act
        var result = await _repository.Search(pagination: new PaginationOptions(1, 5));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(5));
        Assert.That(result.TotalCount, Is.EqualTo(10));
    }
}

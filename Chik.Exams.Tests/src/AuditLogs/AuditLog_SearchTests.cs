using Microsoft.EntityFrameworkCore;

namespace Chik.Exams.Tests.AuditLogs;

[TestFixture]
public class AuditLog_SearchTests
{
    private AuditLogRepository _repository = null!;
    private IDbContextFactory<ChikExamsDbContext> _factory = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        _factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<AuditLogRepository>>();
        _repository = new AuditLogRepository(_factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task Search_WithNoFilter_ShouldReturnAllAuditLogs()
    {
        // Arrange
        var user1 = await TestUtils.CreateTestUser(_factory, "user1");
        var user2 = await TestUtils.CreateTestUser(_factory, "user2");
        await _repository.Create(user1.Id, new AuditLog.Create("User", 1, "{}"));
        await _repository.Create(user2.Id, new AuditLog.Create("Quiz", 2, "{}"));

        // Act
        var result = await _repository.Search();

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Search_WithUserIdFilter_ShouldReturnFilteredAuditLogs()
    {
        // Arrange
        var user1 = await TestUtils.CreateTestUser(_factory, "user1");
        var user2 = await TestUtils.CreateTestUser(_factory, "user2");
        await _repository.Create(user1.Id, new AuditLog.Create("User", 1, "{}"));
        await _repository.Create(user2.Id, new AuditLog.Create("Quiz", 2, "{}"));

        // Act
        var result = await _repository.Search(new AuditLog.Filter(UserId: user1.Id));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Search_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        var user = await TestUtils.CreateTestUser(_factory);
        for (int i = 0; i < 10; i++)
        {
            await _repository.Create(user.Id, new AuditLog.Create("User", i, "{}"));
        }

        // Act
        var result = await _repository.Search(pagination: new PaginationOptions(1, 5));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(5));
        Assert.That(result.TotalCount, Is.EqualTo(10));
    }
}

using Chik.Exams.AuditLogs.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chik.Exams.Tests.AuditLogs;

[TestFixture]
public class AuditLog_GetByUserIdTests
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
    public async Task GetByUserId_WithExistingUserId_ShouldReturnAuditLogs()
    {
        // Arrange
        var user1 = await TestUtils.CreateTestUser(_factory, "user1");
        var user2 = await TestUtils.CreateTestUser(_factory, "user2");
        await _repository.Create(user1.Id, new AuditLog.Create("User", 1, "{}"));
        await _repository.Create(user1.Id, new AuditLog.Create("Quiz", 2, "{}"));
        await _repository.Create(user2.Id, new AuditLog.Create("User", 3, "{}"));

        // Act
        var result = await _repository.GetByUserId(user1.Id);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetByUserId_WithNonExistingUserId_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetByUserId(99999);

        // Assert
        Assert.That(result, Is.Empty);
    }
}

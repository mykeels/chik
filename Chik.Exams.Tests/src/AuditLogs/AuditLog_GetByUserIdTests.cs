using Chik.Exams.AuditLogs.Repositories;

namespace Chik.Exams.Tests.AuditLogs;

[TestFixture]
public class AuditLog_GetByUserIdTests
{
    private AuditLogRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        var factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<AuditLogRepository>>();
        _repository = new AuditLogRepository(factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task GetByUserId_WithExistingUserId_ShouldReturnAuditLogs()
    {
        // Arrange
        await _repository.Create(new AuditLog.Create(1, "User", 1, "{}", "{}", "{}"));
        await _repository.Create(new AuditLog.Create(1, "Quiz", 2, "{}", "{}", "{}"));
        await _repository.Create(new AuditLog.Create(2, "User", 3, "{}", "{}", "{}"));

        // Act
        var result = await _repository.GetByUserId(1);

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

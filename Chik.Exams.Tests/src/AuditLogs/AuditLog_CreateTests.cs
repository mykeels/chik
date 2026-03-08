using Chik.Exams.AuditLogs.Repositories;

namespace Chik.Exams.Tests.AuditLogs;

[TestFixture]
public class AuditLog_CreateTests
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
    public async Task Create_WithValidData_ShouldCreateAuditLog()
    {
        // Arrange
        var create = new AuditLog.Create(
            UserId: 1,
            Entity: "User",
            EntityId: 1,
            ApplicationContext: "{}",
            OldValue: "{}",
            NewValue: "{\"name\": \"test\"}"
        );

        // Act
        var result = await _repository.Create(create);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Entity, Is.EqualTo("User"));
        Assert.That(result.EntityId, Is.EqualTo(1));
    }
}

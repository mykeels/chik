using Microsoft.EntityFrameworkCore;

namespace Chik.Exams.Tests.AuditLogs;

[TestFixture]
public class AuditLog_CreateTests
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
    public async Task Create_WithValidData_ShouldCreateAuditLog()
    {
        // Arrange
        var user = await TestUtils.CreateTestUser(_factory);
        var create = new AuditLog.Create(
            Service: "User",
            EntityId: 1,
            Properties: "{\"name\": \"test\"}"
        );

        // Act
        var result = await _repository.Create(user.Id, create);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Service, Is.EqualTo("User"));
        Assert.That(result.EntityId, Is.EqualTo(1));
    }
}

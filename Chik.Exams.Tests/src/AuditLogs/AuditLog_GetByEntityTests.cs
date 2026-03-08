using Chik.Exams.AuditLogs.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chik.Exams.Tests.AuditLogs;

[TestFixture]
public class AuditLog_GetByEntityTests
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
    public async Task GetByEntity_WithExistingEntity_ShouldReturnAuditLogs()
    {
        // Arrange
        var user = await TestUtils.CreateTestUser(_factory);
        await _repository.Create(user.Id, new AuditLog.Create("User", 1, "{}"));
        await _repository.Create(user.Id, new AuditLog.Create("User", 1, "{}"));
        await _repository.Create(user.Id, new AuditLog.Create("Quiz", 2, "{}"));

        // Act
        var result = await _repository.GetByService("User", 1);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetByEntity_WithNonExistingEntity_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetByService("NonExistent", 99999);

        // Assert
        Assert.That(result, Is.Empty);
    }
}

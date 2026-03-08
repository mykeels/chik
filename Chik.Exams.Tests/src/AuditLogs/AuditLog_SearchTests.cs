using Chik.Exams.AuditLogs.Repositories;

namespace Chik.Exams.Tests.AuditLogs;

[TestFixture]
public class AuditLog_SearchTests
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
    public async Task Search_WithNoFilter_ShouldReturnAllAuditLogs()
    {
        // Arrange
        await _repository.Create(new AuditLog.Create(1, "User", 1, "{}", "{}", "{}"));
        await _repository.Create(new AuditLog.Create(2, "Quiz", 2, "{}", "{}", "{}"));

        // Act
        var result = await _repository.Search();

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Search_WithUserIdFilter_ShouldReturnFilteredAuditLogs()
    {
        // Arrange
        await _repository.Create(new AuditLog.Create(1, "User", 1, "{}", "{}", "{}"));
        await _repository.Create(new AuditLog.Create(2, "Quiz", 2, "{}", "{}", "{}"));

        // Act
        var result = await _repository.Search(new AuditLog.Filter(UserId: 1));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Search_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _repository.Create(new AuditLog.Create(1, "User", i, "{}", "{}", "{}"));
        }

        // Act
        var result = await _repository.Search(pagination: new PaginationOptions(1, 5));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(5));
        Assert.That(result.TotalCount, Is.EqualTo(10));
    }
}

using Chik.Exams.Exams.Repositories;

namespace Chik.Exams.Tests.Exams;

[TestFixture]
public class Exam_StartTests
{
    private ExamRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        var factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<ExamRepository>>();
        _repository = new ExamRepository(factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task Start_WithValidExam_ShouldSetStartedAt()
    {
        // Arrange
        var created = await _repository.Create(new Exam.Create(1, 1, 2));

        // Act
        var result = await _repository.Start(created.Id);

        // Assert
        Assert.That(result.StartedAt, Is.Not.Null);
    }

    [Test]
    public async Task Start_WithAlreadyStartedExam_ShouldThrowException()
    {
        // Arrange
        var created = await _repository.Create(new Exam.Create(1, 1, 2));
        await _repository.Start(created.Id);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository.Start(created.Id));
    }

    [Test]
    public async Task Start_WithNonExistingId_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _repository.Start(99999));
    }
}

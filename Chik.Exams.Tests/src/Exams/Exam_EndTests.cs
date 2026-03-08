using Chik.Exams.Exams.Repositories;

namespace Chik.Exams.Tests.Exams;

[TestFixture]
public class Exam_EndTests
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
    public async Task End_WithStartedExam_ShouldSetEndedAt()
    {
        // Arrange
        var created = await _repository.Create(new Exam.Create(1, 1, 2));
        await _repository.Start(created.Id);

        // Act
        var result = await _repository.End(created.Id);

        // Assert
        Assert.That(result.EndedAt, Is.Not.Null);
    }

    [Test]
    public async Task End_WithNotStartedExam_ShouldThrowException()
    {
        // Arrange
        var created = await _repository.Create(new Exam.Create(1, 1, 2));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository.End(created.Id));
    }

    [Test]
    public async Task End_WithAlreadyEndedExam_ShouldThrowException()
    {
        // Arrange
        var created = await _repository.Create(new Exam.Create(1, 1, 2));
        await _repository.Start(created.Id);
        await _repository.End(created.Id);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository.End(created.Id));
    }

    [Test]
    public async Task End_WithNonExistingId_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _repository.End(99999));
    }
}

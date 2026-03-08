using Chik.Exams.Exams.Repositories;

namespace Chik.Exams.Tests.Exams;

[TestFixture]
public class Exam_MarkTests
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
    public async Task Mark_WithValidData_ShouldSetScoreAndExaminer()
    {
        // Arrange
        var created = await _repository.Create(new Exam.Create(1, 1, 2));

        // Act
        var result = await _repository.Mark(created.Id, score: 85, examinerId: 3, comment: "Good work!");

        // Assert
        Assert.That(result.Score, Is.EqualTo(85));
        Assert.That(result.ExaminerId, Is.EqualTo(3));
        Assert.That(result.ExaminerComment, Is.EqualTo("Good work!"));
    }

    [Test]
    public async Task Mark_WithNonExistingId_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _repository.Mark(99999, 85, 3));
    }
}

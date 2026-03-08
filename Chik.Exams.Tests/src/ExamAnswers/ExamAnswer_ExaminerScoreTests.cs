using Chik.Exams.Exams.ExamAnswers.Repositories;

namespace Chik.Exams.Tests.ExamAnswers;

[TestFixture]
public class ExamAnswer_ExaminerScoreTests
{
    private ExamAnswerRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        var factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<ExamAnswerRepository>>();
        _repository = new ExamAnswerRepository(factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task ExaminerScore_WithValidData_ShouldSetExaminerScoreAndDetails()
    {
        // Arrange
        var created = await _repository.Create(new ExamAnswer.Create(1, 1, "{}"));

        // Act
        var result = await _repository.ExaminerScore(created.Id, score: 8, examinerId: 3, comment: "Good answer");

        // Assert
        Assert.That(result.ExaminerScore, Is.EqualTo(8));
        Assert.That(result.ExaminerId, Is.EqualTo(3));
        Assert.That(result.ExaminerComment, Is.EqualTo("Good answer"));
    }

    [Test]
    public async Task ExaminerScore_WithNonExistingId_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _repository.ExaminerScore(99999, 10, 3));
    }
}

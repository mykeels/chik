using Chik.Exams.Exams.ExamAnswers.Repositories;

namespace Chik.Exams.Tests.ExamAnswers;

[TestFixture]
public class ExamAnswer_AutoScoreTests
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
    public async Task AutoScore_WithValidAnswer_ShouldSetAutoScore()
    {
        // Arrange
        var created = await _repository.Create(new ExamAnswer.Create(1, 1, "{}"));

        // Act
        var result = await _repository.AutoScore(created.Id, 10);

        // Assert
        Assert.That(result.AutoScore, Is.EqualTo(10));
    }

    [Test]
    public async Task AutoScore_WithNonExistingId_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _repository.AutoScore(99999, 10));
    }
}

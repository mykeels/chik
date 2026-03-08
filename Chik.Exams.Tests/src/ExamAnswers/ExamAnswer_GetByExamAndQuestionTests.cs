using Chik.Exams.Exams.ExamAnswers.Repositories;

namespace Chik.Exams.Tests.ExamAnswers;

[TestFixture]
public class ExamAnswer_GetByExamAndQuestionTests
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
    public async Task GetByExamAndQuestion_WithExistingAnswer_ShouldReturnAnswer()
    {
        // Arrange
        await _repository.Create(new ExamAnswer.Create(1, 1, "{}"));

        // Act
        var result = await _repository.GetByExamAndQuestion(1, 1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ExamId, Is.EqualTo(1));
        Assert.That(result.QuestionId, Is.EqualTo(1));
    }

    [Test]
    public async Task GetByExamAndQuestion_WithNonExistingAnswer_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByExamAndQuestion(99999, 99999);

        // Assert
        Assert.That(result, Is.Null);
    }
}

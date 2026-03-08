using Chik.Exams.Exams.ExamAnswers.Repositories;

namespace Chik.Exams.Tests.ExamAnswers;

[TestFixture]
public class ExamAnswer_CreateTests
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
    public async Task Create_WithValidData_ShouldCreateExamAnswer()
    {
        // Arrange
        var create = new ExamAnswer.Create(ExamId: 1, QuestionId: 1, Answer: "{\"answer\": \"Option A\"}");

        // Act
        var result = await _repository.Create(create);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ExamId, Is.EqualTo(1));
        Assert.That(result.QuestionId, Is.EqualTo(1));
    }
}

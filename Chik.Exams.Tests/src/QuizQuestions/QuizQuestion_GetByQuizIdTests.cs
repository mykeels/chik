
namespace Chik.Exams.Tests.QuizQuestions;

[TestFixture]
public class QuizQuestion_GetByQuizIdTests
{
    private QuizQuestionRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        var factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<QuizQuestionRepository>>();
        _repository = new QuizQuestionRepository(factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task GetByQuizId_WithExistingQuizId_ShouldReturnQuestions()
    {
        // Arrange
        await _repository.Create(new QuizQuestion.Create(1, "Question 1", 1, "{}", 10, 1));
        await _repository.Create(new QuizQuestion.Create(1, "Question 2", 1, "{}", 10, 2));
        await _repository.Create(new QuizQuestion.Create(2, "Question 3", 1, "{}", 10, 1));

        // Act
        var result = await _repository.GetByQuizId(1);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetByQuizId_WithIncludeDeactivated_ShouldIncludeDeactivatedQuestions()
    {
        // Arrange
        var q1 = await _repository.Create(new QuizQuestion.Create(1, "Question 1", 1, "{}", 10, 1));
        await _repository.Create(new QuizQuestion.Create(1, "Question 2", 1, "{}", 10, 2));
        await _repository.Deactivate(q1.Id);

        // Act
        var resultWithDeactivated = await _repository.GetByQuizId(1, includeDeactivated: true);
        var resultWithoutDeactivated = await _repository.GetByQuizId(1, includeDeactivated: false);

        // Assert
        Assert.That(resultWithDeactivated, Has.Count.EqualTo(2));
        Assert.That(resultWithoutDeactivated, Has.Count.EqualTo(1));
    }
}


namespace Chik.Exams.Tests.QuizQuestions;

[TestFixture]
public class QuizQuestion_ReorderQuestionsTests
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
    public async Task ReorderQuestions_WithValidOrder_ShouldReorderQuestions()
    {
        // Arrange
        var q1 = await _repository.Create(new QuizQuestion.Create(1, "Question 1", 1, "{}", 10, 1));
        var q2 = await _repository.Create(new QuizQuestion.Create(1, "Question 2", 1, "{}", 10, 2));
        var q3 = await _repository.Create(new QuizQuestion.Create(1, "Question 3", 1, "{}", 10, 3));

        // Act
        await _repository.ReorderQuestions(1, [q3.Id, q1.Id, q2.Id]);

        // Assert
        var questions = await _repository.GetByQuizId(1);
        Assert.That(questions[0].Id, Is.EqualTo(q3.Id));
        Assert.That(questions[1].Id, Is.EqualTo(q1.Id));
        Assert.That(questions[2].Id, Is.EqualTo(q2.Id));
    }
}

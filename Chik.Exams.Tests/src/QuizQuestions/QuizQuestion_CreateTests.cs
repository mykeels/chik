
namespace Chik.Exams.Tests.QuizQuestions;

[TestFixture]
public class QuizQuestion_CreateTests
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
    public async Task Create_WithValidData_ShouldCreateQuestion()
    {
        // Arrange
        var create = new QuizQuestion.Create(
            QuizId: 1,
            Prompt: "What is 2+2?",
            TypeId: QuizQuestionType.Types.SingleChoice,
            Properties: "{}",
            Score: 10,
            Order: 1
        );

        // Act
        var result = await _repository.Create(create);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Prompt, Is.EqualTo("What is 2+2?"));
        Assert.That(result.Score, Is.EqualTo(10));
    }
}

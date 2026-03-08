using Chik.Exams.Quizzes.QuestionTypes.Repositories;

namespace Chik.Exams.Tests.QuizQuestionTypes;

[TestFixture]
public class QuizQuestionType_GetAllTests
{
    private QuizQuestionTypeRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        var factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<QuizQuestionTypeRepository>>();
        _repository = new QuizQuestionTypeRepository(factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task GetAll_ShouldReturnAllQuestionTypes()
    {
        // Arrange
        await _repository.Create(new QuizQuestionType.Create("Multiple Choice", "Description"));
        await _repository.Create(new QuizQuestionType.Create("Single Choice", "Description"));
        await _repository.Create(new QuizQuestionType.Create("Essay", "Description"));

        // Act
        var result = await _repository.GetAll();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task GetAll_WithNoTypes_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetAll();

        // Assert
        Assert.That(result, Is.Empty);
    }
}

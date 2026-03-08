using Chik.Exams.Quizzes.QuestionTypes.Repositories;

namespace Chik.Exams.Tests.QuizQuestionTypes;

[TestFixture]
public class QuizQuestionType_SearchTests
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
    public async Task Search_WithNoFilter_ShouldReturnAllQuestionTypes()
    {
        // Arrange
        await _repository.Create(new QuizQuestionType.Create("Multiple Choice", "Description"));
        await _repository.Create(new QuizQuestionType.Create("Single Choice", "Description"));

        // Act
        var result = await _repository.Search();

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Search_WithNameFilter_ShouldReturnFilteredQuestionTypes()
    {
        // Arrange
        await _repository.Create(new QuizQuestionType.Create("Multiple Choice", "Description"));
        await _repository.Create(new QuizQuestionType.Create("Single Choice", "Description"));

        // Act
        var result = await _repository.Search(new QuizQuestionType.Filter(Name: "Multiple"));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(1));
    }
}

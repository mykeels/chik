
namespace Chik.Exams.Tests.QuizQuestions;

[TestFixture]
public class QuizQuestion_SearchTests
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
    public async Task Search_WithNoFilter_ShouldReturnAllQuestions()
    {
        // Arrange
        await _repository.Create(new QuizQuestion.Create(1, "Question 1", 1, "{}", 10, 1));
        await _repository.Create(new QuizQuestion.Create(1, "Question 2", 1, "{}", 10, 2));

        // Act
        var result = await _repository.Search();

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Search_WithQuizIdFilter_ShouldReturnFilteredQuestions()
    {
        // Arrange
        await _repository.Create(new QuizQuestion.Create(1, "Question 1", 1, "{}", 10, 1));
        await _repository.Create(new QuizQuestion.Create(2, "Question 2", 1, "{}", 10, 1));

        // Act
        var result = await _repository.Search(new QuizQuestion.Filter(QuizId: 1));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Search_WithIsActiveFilter_ShouldReturnFilteredQuestions()
    {
        // Arrange
        var q1 = await _repository.Create(new QuizQuestion.Create(1, "Question 1", 1, "{}", 10, 1));
        await _repository.Create(new QuizQuestion.Create(1, "Question 2", 1, "{}", 10, 2));
        await _repository.Deactivate(q1.Id);

        // Act
        var activeOnly = await _repository.Search(new QuizQuestion.Filter(IsActive: true));
        var inactiveOnly = await _repository.Search(new QuizQuestion.Filter(IsActive: false));

        // Assert
        Assert.That(activeOnly.Items, Has.Count.EqualTo(1));
        Assert.That(inactiveOnly.Items, Has.Count.EqualTo(1));
    }
}

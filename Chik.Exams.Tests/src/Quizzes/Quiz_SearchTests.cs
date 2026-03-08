using Chik.Exams.Quizzes.Repositories;

namespace Chik.Exams.Tests.Quizzes;

[TestFixture]
public class Quiz_SearchTests
{
    private QuizRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        var factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<QuizRepository>>();
        _repository = new QuizRepository(factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task Search_WithNoFilter_ShouldReturnAllQuizzes()
    {
        // Arrange
        await _repository.Create(new Quiz.Create("Quiz 1", "Description 1", 1));
        await _repository.Create(new Quiz.Create("Quiz 2", "Description 2", 1));

        // Act
        var result = await _repository.Search();

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Search_WithTitleFilter_ShouldReturnFilteredQuizzes()
    {
        // Arrange
        await _repository.Create(new Quiz.Create("Math Quiz", "Description", 1));
        await _repository.Create(new Quiz.Create("Science Quiz", "Description", 1));

        // Act
        var result = await _repository.Search(new Quiz.Filter(Title: "Math"));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.Items[0].Title, Is.EqualTo("Math Quiz"));
    }

    [Test]
    public async Task Search_WithCreatorIdFilter_ShouldReturnFilteredQuizzes()
    {
        // Arrange
        await _repository.Create(new Quiz.Create("Quiz 1", "Description", 1));
        await _repository.Create(new Quiz.Create("Quiz 2", "Description", 2));

        // Act
        var result = await _repository.Search(new Quiz.Filter(CreatorId: 1));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Search_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _repository.Create(new Quiz.Create($"Quiz {i}", "Description", 1));
        }

        // Act
        var result = await _repository.Search(pagination: new PaginationOptions(1, 5));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(5));
        Assert.That(result.TotalCount, Is.EqualTo(10));
    }
}

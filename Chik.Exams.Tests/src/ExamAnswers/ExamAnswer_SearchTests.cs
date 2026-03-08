using Chik.Exams.Exams.ExamAnswers.Repositories;

namespace Chik.Exams.Tests.ExamAnswers;

[TestFixture]
public class ExamAnswer_SearchTests
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
    public async Task Search_WithNoFilter_ShouldReturnAllAnswers()
    {
        // Arrange
        await _repository.Create(new ExamAnswer.Create(1, 1, "{}"));
        await _repository.Create(new ExamAnswer.Create(1, 2, "{}"));

        // Act
        var result = await _repository.Search();

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Search_WithExamIdFilter_ShouldReturnFilteredAnswers()
    {
        // Arrange
        await _repository.Create(new ExamAnswer.Create(1, 1, "{}"));
        await _repository.Create(new ExamAnswer.Create(2, 1, "{}"));

        // Act
        var result = await _repository.Search(new ExamAnswer.Filter(ExamId: 1));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Search_WithIsAutoScoredFilter_ShouldReturnFilteredAnswers()
    {
        // Arrange
        var answer1 = await _repository.Create(new ExamAnswer.Create(1, 1, "{}"));
        await _repository.Create(new ExamAnswer.Create(1, 2, "{}"));
        await _repository.AutoScore(answer1.Id, 10);

        // Act
        var scoredAnswers = await _repository.Search(new ExamAnswer.Filter(IsAutoScored: true));
        var unscoredAnswers = await _repository.Search(new ExamAnswer.Filter(IsAutoScored: false));

        // Assert
        Assert.That(scoredAnswers.Items, Has.Count.EqualTo(1));
        Assert.That(unscoredAnswers.Items, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Search_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _repository.Create(new ExamAnswer.Create(1, i, "{}"));
        }

        // Act
        var result = await _repository.Search(pagination: new PaginationOptions(1, 5));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(5));
        Assert.That(result.TotalCount, Is.EqualTo(10));
    }
}

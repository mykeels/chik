
namespace Chik.Exams.Tests.Exams;

[TestFixture]
public class Exam_SearchTests
{
    private ExamRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        var factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<ExamRepository>>();
        _repository = new ExamRepository(factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task Search_WithNoFilter_ShouldReturnAllExams()
    {
        // Arrange
        await _repository.Create(new Exam.Create(1, 1, 2, 0));
        await _repository.Create(new Exam.Create(2, 2, 2, 0));

        // Act
        var result = await _repository.Search();

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Search_WithUserIdFilter_ShouldReturnFilteredExams()
    {
        // Arrange
        await _repository.Create(new Exam.Create(1, 1, 2, 0));
        await _repository.Create(new Exam.Create(2, 1, 2, 0));

        // Act
        var result = await _repository.Search(new Exam.Filter(UserId: 1));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Search_WithIsStartedFilter_ShouldReturnFilteredExams()
    {
        // Arrange
        var exam1 = await _repository.Create(new Exam.Create(1, 1, 2, 0));
        await _repository.Create(new Exam.Create(2, 1, 2, 0));
        await _repository.Start(exam1.Id);

        // Act
        var startedExams = await _repository.Search(new Exam.Filter(IsStarted: true));
        var notStartedExams = await _repository.Search(new Exam.Filter(IsStarted: false));

        // Assert
        Assert.That(startedExams.Items, Has.Count.EqualTo(1));
        Assert.That(notStartedExams.Items, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Search_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _repository.Create(new Exam.Create(i, 1, 2, 0));
        }

        // Act
        var result = await _repository.Search(pagination: new PaginationOptions(1, 5));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(5));
        Assert.That(result.TotalCount, Is.EqualTo(10));
    }
}


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
        // Act (DB is seeded with 6 types)
        var result = await _repository.Search();

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(6));
    }

    [Test]
    public async Task Search_WithNameFilter_ShouldReturnFilteredQuestionTypes()
    {
        // Arrange
        await _repository.Create(new QuizQuestionType.Create("Test Multiple", "Description"));
        await _repository.Create(new QuizQuestionType.Create("Test Single", "Description"));

        // Act
        var result = await _repository.Search(new QuizQuestionType.Filter(Name: "Test Multiple"));

        // Assert
        Assert.That(result.Items, Has.Count.EqualTo(1));
    }
}

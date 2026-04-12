
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
        // Act (DB is seeded with 6 types: Multiple Choice, Single Choice, Fill in the Blank, Essay, Short Answer, True or False)
        var result = await _repository.GetAll();

        // Assert
        Assert.That(result, Has.Count.EqualTo(6));
    }

    [Test]
    public async Task GetAll_WithAdditionalType_ShouldIncludeIt()
    {
        // Arrange
        await _repository.Create(new QuizQuestionType.Create("Test Type", "Description"));

        // Act
        var result = await _repository.GetAll();

        // Assert
        Assert.That(result, Has.Count.EqualTo(7));
    }
}

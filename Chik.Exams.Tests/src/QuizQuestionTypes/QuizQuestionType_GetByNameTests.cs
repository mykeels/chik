
namespace Chik.Exams.Tests.QuizQuestionTypes;

[TestFixture]
public class QuizQuestionType_GetByNameTests
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
    public async Task GetByName_WithExistingName_ShouldReturnQuestionType()
    {
        // Act (uses seeded "Multiple Choice" type)
        var result = await _repository.GetByName("Multiple Choice");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Multiple Choice"));
    }

    [Test]
    public async Task GetByName_WithNonExistingName_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByName("NonExistent");

        // Assert
        Assert.That(result, Is.Null);
    }
}

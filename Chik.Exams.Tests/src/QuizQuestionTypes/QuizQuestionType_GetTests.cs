
namespace Chik.Exams.Tests.QuizQuestionTypes;

[TestFixture]
public class QuizQuestionType_GetTests
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
    public async Task Get_WithExistingId_ShouldReturnQuestionType()
    {
        // Arrange
        var created = await _repository.Create(new QuizQuestionType.Create("Test Type", "Description"));

        // Act
        var result = await _repository.Get(created.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(created.Id));
    }

    [Test]
    public async Task Get_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.Get(99999);

        // Assert
        Assert.That(result, Is.Null);
    }
}

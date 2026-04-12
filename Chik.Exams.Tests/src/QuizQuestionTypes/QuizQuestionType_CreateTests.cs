
namespace Chik.Exams.Tests.QuizQuestionTypes;

[TestFixture]
public class QuizQuestionType_CreateTests
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
    public async Task Create_WithValidData_ShouldCreateQuestionType()
    {
        // Arrange
        var create = new QuizQuestionType.Create("Test Type", "Select one or more options");

        // Act
        var result = await _repository.Create(create);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Test Type"));
    }

    [Test]
    public async Task Create_WithDuplicateName_ShouldThrowException()
    {
        // Arrange
        await _repository.Create(new QuizQuestionType.Create("Test Type", "Description"));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository.Create(new QuizQuestionType.Create("Test Type", "Another description")));
    }
}

using Chik.Exams.Quizzes.QuestionTypes.Repositories;

namespace Chik.Exams.Tests.QuizQuestionTypes;

[TestFixture]
public class QuizQuestionType_DeleteTests
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
    public async Task Delete_WithExistingId_ShouldDeleteQuestionType()
    {
        // Arrange
        var created = await _repository.Create(new QuizQuestionType.Create("Multiple Choice", "Description"));

        // Act
        await _repository.Delete(created.Id);

        // Assert
        var result = await _repository.Get(created.Id);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Delete_WithNonExistingId_ShouldNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _repository.Delete(99999));
    }
}

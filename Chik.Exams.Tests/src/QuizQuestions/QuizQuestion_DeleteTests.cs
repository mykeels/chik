namespace Chik.Exams.Tests.QuizQuestions;

[TestFixture]
public class QuizQuestion_DeleteTests
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
    public async Task Delete_WithExistingId_ShouldDeleteQuestion()
    {
        // Arrange
        var created = await _repository.Create(new QuizQuestion.Create(1, "Question", 1, "{}", 10, 1));

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

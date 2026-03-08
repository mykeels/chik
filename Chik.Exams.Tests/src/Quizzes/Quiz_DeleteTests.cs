using Chik.Exams.Quizzes.Repositories;

namespace Chik.Exams.Tests.Quizzes;

[TestFixture]
public class Quiz_DeleteTests
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
    public async Task Delete_WithExistingId_ShouldDeleteQuiz()
    {
        // Arrange
        var created = await _repository.Create(new Quiz.Create("Test Quiz", "Description", 1));

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

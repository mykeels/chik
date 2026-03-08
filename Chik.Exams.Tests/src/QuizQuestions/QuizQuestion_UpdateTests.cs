using Chik.Exams.Quizzes.Questions.Repositories;

namespace Chik.Exams.Tests.QuizQuestions;

[TestFixture]
public class QuizQuestion_UpdateTests
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
    public async Task Update_WithValidData_ShouldUpdateQuestion()
    {
        // Arrange
        var created = await _repository.Create(new QuizQuestion.Create(1, "Original prompt", 1, "{}", 10, 1));

        // Act
        var result = await _repository.Update(created.Id, new QuizQuestion.Update(created.Id, Prompt: "Updated prompt"));

        // Assert
        Assert.That(result.Prompt, Is.EqualTo("Updated prompt"));
    }

    [Test]
    public async Task Update_WithNonExistingId_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _repository.Update(99999, new QuizQuestion.Update(99999, Prompt: "Updated")));
    }

    [Test]
    public async Task Update_Score_ShouldUpdateScore()
    {
        // Arrange
        var created = await _repository.Create(new QuizQuestion.Create(1, "Question", 1, "{}", 10, 1));

        // Act
        var result = await _repository.Update(created.Id, new QuizQuestion.Update(created.Id, Score: 20));

        // Assert
        Assert.That(result.Score, Is.EqualTo(20));
    }
}

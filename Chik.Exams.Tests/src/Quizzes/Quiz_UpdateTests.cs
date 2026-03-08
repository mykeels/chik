using Chik.Exams.Quizzes.Repositories;

namespace Chik.Exams.Tests.Quizzes;

[TestFixture]
public class Quiz_UpdateTests
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
    public async Task Update_WithValidData_ShouldUpdateQuiz()
    {
        // Arrange
        var created = await _repository.Create(new Quiz.Create("Test Quiz", "Description", 1));

        // Act
        var result = await _repository.Update(created.Id, new Quiz.Update(created.Id, Title: "Updated Quiz"));

        // Assert
        Assert.That(result.Title, Is.EqualTo("Updated Quiz"));
    }

    [Test]
    public async Task Update_WithNonExistingId_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _repository.Update(99999, new Quiz.Update(99999, Title: "Updated")));
    }

    [Test]
    public async Task Update_Duration_ShouldUpdateDuration()
    {
        // Arrange
        var created = await _repository.Create(new Quiz.Create("Test Quiz", "Description", 1));

        // Act
        var result = await _repository.Update(created.Id, new Quiz.Update(created.Id, Duration: TimeSpan.FromHours(1)));

        // Assert
        Assert.That(result.Duration, Is.EqualTo(TimeSpan.FromHours(1)));
    }
}

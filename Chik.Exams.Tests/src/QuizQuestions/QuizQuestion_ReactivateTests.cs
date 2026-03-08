using Chik.Exams.Quizzes.Questions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chik.Exams.Tests.QuizQuestions;

[TestFixture]
public class QuizQuestion_ReactivateTests
{
    private QuizQuestionRepository _repository = null!;
    private IDbContextFactory<ChikExamsDbContext> _factory = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        _factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<QuizQuestionRepository>>();
        _repository = new QuizQuestionRepository(_factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task Reactivate_WithDeactivatedQuestion_ShouldReactivateQuestion()
    {
        // Arrange
        var user = await TestUtils.CreateTestUser(_factory);
        var quiz = await TestUtils.CreateTestQuiz(_factory, user.Id);
        var questionType = await TestUtils.CreateTestQuizQuestionType(_factory);
        var created = await _repository.Create(new QuizQuestion.Create(quiz.Id, "Question", questionType.Id, "{}", 10, 1));
        await _repository.Deactivate(created.Id);

        // Act
        await _repository.Reactivate(created.Id);

        // Assert
        var result = await _repository.Get(created.Id);
        Assert.That(result!.DeactivatedAt, Is.Null);
    }

    [Test]
    public async Task Reactivate_WithNonExistingId_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _repository.Reactivate(99999));
    }
}

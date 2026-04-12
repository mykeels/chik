using Microsoft.EntityFrameworkCore;

namespace Chik.Exams.Tests.QuizQuestions;

[TestFixture]
public class QuizQuestion_GetTests
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
    public async Task Get_WithExistingId_ShouldReturnQuestion()
    {
        // Arrange
        var user = await TestUtils.CreateTestUser(_factory);
        var quiz = await TestUtils.CreateTestQuiz(_factory, user.Id);
        var questionType = await TestUtils.CreateTestQuizQuestionType(_factory);
        var created = await _repository.Create(new QuizQuestion.Create(quiz.Id, "What is 2+2?", questionType.Id, "{}", 10, 1));

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

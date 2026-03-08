using Chik.Exams.Quizzes.Repositories;

namespace Chik.Exams.Tests.Quizzes;

[TestFixture]
public class Quiz_GetTests
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
    public async Task Get_WithExistingId_ShouldReturnQuiz()
    {
        // Arrange
        var created = await _repository.Create(new Quiz.Create("Test Quiz", "Description", 1));

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

    [Test]
    public async Task Get_WithIncludeQuestions_ShouldIncludeQuestions()
    {
        // Arrange
        var created = await _repository.Create(new Quiz.Create("Test Quiz", "Description", 1));
        // Note: Questions would need to be added separately

        // Act
        var result = await _repository.Get(created.Id, includeQuestions: true);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Questions, Is.Not.Null);
    }
}

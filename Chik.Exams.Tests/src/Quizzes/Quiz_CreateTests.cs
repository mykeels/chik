
namespace Chik.Exams.Tests.Quizzes;

[TestFixture]
public class Quiz_CreateTests
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
    public async Task Create_WithValidData_ShouldCreateQuiz()
    {
        // Arrange
        var create = new Quiz.Create("Test Quiz", "A test quiz description", 1);

        // Act
        var result = await _repository.Create(create);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Test Quiz"));
        Assert.That(result.Description, Is.EqualTo("A test quiz description"));
    }

    [Test]
    public async Task Create_WithDuration_ShouldCreateQuizWithDuration()
    {
        // Arrange
        var create = new Quiz.Create("Timed Quiz", "A timed quiz", 1, Duration: TimeSpan.FromMinutes(30));

        // Act
        var result = await _repository.Create(create);

        // Assert
        Assert.That(result.Duration, Is.EqualTo(TimeSpan.FromMinutes(30)));
    }

    [Test]
    public async Task Create_WithExaminer_ShouldCreateQuizWithExaminer()
    {
        // Arrange
        var create = new Quiz.Create("Quiz with Examiner", "A quiz with assigned examiner", 1, ExaminerId: 2);

        // Act
        var result = await _repository.Create(create);

        // Assert
        Assert.That(result.ExaminerId, Is.EqualTo(2));
    }
}

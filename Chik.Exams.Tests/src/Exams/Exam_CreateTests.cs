namespace Chik.Exams.Tests.Exams;

[TestFixture]
public class Exam_CreateTests
{
    private ExamRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        var factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<ExamRepository>>();
        _repository = new ExamRepository(factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task Create_WithValidData_ShouldCreateExam()
    {
        // Arrange
        var create = new Exam.Create(UserId: 1, QuizId: 1, CreatorId: 2, StudentClassId: 1);

        // Act
        var result = await _repository.Create(create);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(1));
        Assert.That(result.QuizId, Is.EqualTo(1));
        Assert.That(result.CreatorId, Is.EqualTo(2));
    }
}

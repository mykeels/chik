using Microsoft.EntityFrameworkCore;

namespace Chik.Exams.Tests.Exams;

[TestFixture]
public class Exam_GetByQuizIdTests
{
    private ExamRepository _repository = null!;
    private IDbContextFactory<ChikExamsDbContext> _factory = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        _factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<ExamRepository>>();
        _repository = new ExamRepository(_factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task GetByQuizId_WithExistingQuizId_ShouldReturnExams()
    {
        // Arrange
        var user1 = await TestUtils.CreateTestUser(_factory, "student1");
        var user2 = await TestUtils.CreateTestUser(_factory, "student2");
        var user3 = await TestUtils.CreateTestUser(_factory, "student3");
        var creator = await TestUtils.CreateTestUser(_factory, "creator");
        var quiz1 = await TestUtils.CreateTestQuiz(_factory, creator.Id, "Quiz 1");
        var quiz2 = await TestUtils.CreateTestQuiz(_factory, creator.Id, "Quiz 2");
        
        await _repository.Create(new Exam.Create(user1.Id, quiz1.Id, creator.Id, 0));
        await _repository.Create(new Exam.Create(user2.Id, quiz1.Id, creator.Id, 0));
        await _repository.Create(new Exam.Create(user3.Id, quiz2.Id, creator.Id, 0));

        // Act
        var result = await _repository.GetByQuizId(quiz1.Id);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetByQuizId_WithNonExistingQuizId_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetByQuizId(99999);

        // Assert
        Assert.That(result, Is.Empty);
    }
}

using Chik.Exams.Exams.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chik.Exams.Tests.Exams;

[TestFixture]
public class Exam_GetTests
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
    public async Task Get_WithExistingId_ShouldReturnExam()
    {
        // Arrange
        var user = await TestUtils.CreateTestUser(_factory, "student");
        var creator = await TestUtils.CreateTestUser(_factory, "creator");
        var quiz = await TestUtils.CreateTestQuiz(_factory, creator.Id);
        var created = await _repository.Create(new Exam.Create(user.Id, quiz.Id, creator.Id));

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
    public async Task Get_WithIncludeAnswers_ShouldIncludeAnswers()
    {
        // Arrange
        var user = await TestUtils.CreateTestUser(_factory, "student");
        var creator = await TestUtils.CreateTestUser(_factory, "creator");
        var quiz = await TestUtils.CreateTestQuiz(_factory, creator.Id);
        var created = await _repository.Create(new Exam.Create(user.Id, quiz.Id, creator.Id));

        // Act
        var result = await _repository.Get(created.Id, includeAnswers: true);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Answers, Is.Not.Null);
    }
}

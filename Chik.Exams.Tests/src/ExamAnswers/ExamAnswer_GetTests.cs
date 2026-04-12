using Microsoft.EntityFrameworkCore;

namespace Chik.Exams.Tests.ExamAnswers;

[TestFixture]
public class ExamAnswer_GetTests
{
    private ExamAnswerRepository _repository = null!;
    private IDbContextFactory<ChikExamsDbContext> _factory = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        _factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<ExamAnswerRepository>>();
        _repository = new ExamAnswerRepository(_factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task Get_WithExistingId_ShouldReturnExamAnswer()
    {
        // Arrange
        var user = await TestUtils.CreateTestUser(_factory, "student");
        var creator = await TestUtils.CreateTestUser(_factory, "creator");
        var quiz = await TestUtils.CreateTestQuiz(_factory, creator.Id);
        var questionType = await TestUtils.CreateTestQuizQuestionType(_factory);
        var question = await TestUtils.CreateTestQuizQuestion(_factory, quiz.Id, questionType.Id);
        var exam = await TestUtils.CreateTestExam(_factory, user.Id, quiz.Id, creator.Id);
        var created = await _repository.Create(new ExamAnswer.Create(exam.Id, question.Id, "{}"));

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

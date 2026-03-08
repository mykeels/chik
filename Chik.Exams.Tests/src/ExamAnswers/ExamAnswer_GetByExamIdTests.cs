using Chik.Exams.Exams.ExamAnswers.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chik.Exams.Tests.ExamAnswers;

[TestFixture]
public class ExamAnswer_GetByExamIdTests
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
    public async Task GetByExamId_WithExistingExamId_ShouldReturnAnswers()
    {
        // Arrange
        var user = await TestUtils.CreateTestUser(_factory, "student");
        var creator = await TestUtils.CreateTestUser(_factory, "creator");
        var quiz = await TestUtils.CreateTestQuiz(_factory, creator.Id);
        var questionType = await TestUtils.CreateTestQuizQuestionType(_factory);
        var question1 = await TestUtils.CreateTestQuizQuestion(_factory, quiz.Id, questionType.Id, "Question 1");
        var question2 = await TestUtils.CreateTestQuizQuestion(_factory, quiz.Id, questionType.Id, "Question 2");
        var exam1 = await TestUtils.CreateTestExam(_factory, user.Id, quiz.Id, creator.Id);
        var exam2 = await TestUtils.CreateTestExam(_factory, user.Id, quiz.Id, creator.Id);
        
        await _repository.Create(new ExamAnswer.Create(exam1.Id, question1.Id, "{}"));
        await _repository.Create(new ExamAnswer.Create(exam1.Id, question2.Id, "{}"));
        await _repository.Create(new ExamAnswer.Create(exam2.Id, question1.Id, "{}"));

        // Act
        var result = await _repository.GetByExamId(exam1.Id);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetByExamId_WithNonExistingExamId_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetByExamId(99999);

        // Assert
        Assert.That(result, Is.Empty);
    }
}

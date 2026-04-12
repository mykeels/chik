using Microsoft.EntityFrameworkCore;

namespace Chik.Exams.Tests.ExamAnswers;

[TestFixture]
public class ExamAnswer_SubmitAnswerTests
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
    public async Task SubmitAnswer_WithNewAnswer_ShouldCreateAnswer()
    {
        // Arrange
        var user = await TestUtils.CreateTestUser(_factory, "student");
        var creator = await TestUtils.CreateTestUser(_factory, "creator");
        var quiz = await TestUtils.CreateTestQuiz(_factory, creator.Id);
        var questionType = await TestUtils.CreateTestQuizQuestionType(_factory);
        var question = await TestUtils.CreateTestQuizQuestion(_factory, quiz.Id, questionType.Id);
        var exam = await TestUtils.CreateTestExam(_factory, user.Id, quiz.Id, creator.Id);

        // Act
        var result = await _repository.SubmitAnswer(exam.Id, question.Id, "{\"answer\": \"A\"}");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Answer, Is.EqualTo("{\"answer\": \"A\"}"));
    }

    [Test]
    public async Task SubmitAnswer_WithExistingAnswer_ShouldUpdateAnswer()
    {
        // Arrange
        var user = await TestUtils.CreateTestUser(_factory, "student");
        var creator = await TestUtils.CreateTestUser(_factory, "creator");
        var quiz = await TestUtils.CreateTestQuiz(_factory, creator.Id);
        var questionType = await TestUtils.CreateTestQuizQuestionType(_factory);
        var question = await TestUtils.CreateTestQuizQuestion(_factory, quiz.Id, questionType.Id);
        var exam = await TestUtils.CreateTestExam(_factory, user.Id, quiz.Id, creator.Id);
        
        await _repository.SubmitAnswer(exam.Id, question.Id, "{\"answer\": \"A\"}");

        // Act
        var result = await _repository.SubmitAnswer(exam.Id, question.Id, "{\"answer\": \"B\"}");

        // Assert
        Assert.That(result.Answer, Is.EqualTo("{\"answer\": \"B\"}"));

        // Verify only one answer exists
        var answers = await _repository.GetByExamId(exam.Id);
        Assert.That(answers, Has.Count.EqualTo(1));
    }
}

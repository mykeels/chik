
namespace Chik.Exams.Tests.ExamAnswers;

[TestFixture]
public class ExamAnswer_UpdateTests
{
    private ExamAnswerRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        var factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<ExamAnswerRepository>>();
        _repository = new ExamAnswerRepository(factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task Update_WithValidData_ShouldUpdateExamAnswer()
    {
        // Arrange
        var created = await _repository.Create(new ExamAnswer.Create(1, 1, "{\"old\": true}"));

        // Act
        var result = await _repository.Update(created.Id, new ExamAnswer.Update(created.Id, Answer: "{\"new\": true}"));

        // Assert
        Assert.That(result.Answer, Is.EqualTo("{\"new\": true}"));
    }

    [Test]
    public async Task Update_WithNonExistingId_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _repository.Update(99999, new ExamAnswer.Update(99999, Answer: "{}")));
    }
}

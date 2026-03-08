using Chik.Exams.Exams.ExamAnswers.Repositories;

namespace Chik.Exams.Tests.ExamAnswers;

[TestFixture]
public class ExamAnswer_DeleteTests
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
    public async Task Delete_WithExistingId_ShouldDeleteAnswer()
    {
        // Arrange
        var created = await _repository.Create(new ExamAnswer.Create(1, 1, "{}"));

        // Act
        await _repository.Delete(created.Id);

        // Assert
        var result = await _repository.Get(created.Id);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Delete_WithNonExistingId_ShouldNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _repository.Delete(99999));
    }
}

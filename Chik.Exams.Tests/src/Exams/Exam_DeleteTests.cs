
namespace Chik.Exams.Tests.Exams;

[TestFixture]
public class Exam_DeleteTests
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
    public async Task Delete_WithExistingId_ShouldDeleteExam()
    {
        // Arrange
        var created = await _repository.Create(new Exam.Create(1, 1, 2, 0));

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

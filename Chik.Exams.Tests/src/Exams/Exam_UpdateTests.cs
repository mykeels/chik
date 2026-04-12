
namespace Chik.Exams.Tests.Exams;

[TestFixture]
public class Exam_UpdateTests
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
    public async Task Update_WithValidData_ShouldUpdateExam()
    {
        // Arrange
        var created = await _repository.Create(new Exam.Create(1, 1, 2, 0));

        // Act
        var result = await _repository.Update(created.Id, new Exam.Update(created.Id, ExaminerComment: "Good job!"));

        // Assert
        Assert.That(result.ExaminerComment, Is.EqualTo("Good job!"));
    }

    [Test]
    public async Task Update_WithNonExistingId_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _repository.Update(99999, new Exam.Update(99999, ExaminerComment: "Test")));
    }
}

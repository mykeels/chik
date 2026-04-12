
namespace Chik.Exams.Tests.QuizQuestionTypes;

[TestFixture]
public class QuizQuestionType_UpdateTests
{
    private QuizQuestionTypeRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        TestSetup.EnsureDatabaseReset();
        var factory = TestSetup.DbContextFactory();
        var logger = new Mock<ILogger<QuizQuestionTypeRepository>>();
        _repository = new QuizQuestionTypeRepository(factory, logger.Object, TestUtils.TimeProvider);
    }

    [Test]
    public async Task Update_WithValidData_ShouldUpdateQuestionType()
    {
        // Arrange
        var created = await _repository.Create(new QuizQuestionType.Create("Test Type", "Old description"));

        // Act
        var result = await _repository.Update(created.Id, new QuizQuestionType.Update(created.Id, Description: "New description"));

        // Assert
        Assert.That(result.Description, Is.EqualTo("New description"));
    }

    [Test]
    public async Task Update_WithNonExistingId_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _repository.Update(99999, new QuizQuestionType.Update(99999, Name: "Updated")));
    }
}

using Microsoft.AspNetCore.Mvc;

namespace Chik.Exams.Api;

[ApiController]
[Route("api/quiz-questions")]
public class QuizQuestionsController : ControllerBase
{
    private readonly IQuizQuestionService _quizQuestionService;

    public QuizQuestionsController(IQuizQuestionService quizQuestionService)
    {
        _quizQuestionService = quizQuestionService;
    }

    /// <summary>
    /// Gets a quiz question by ID.
    /// </summary>
    [HttpGet("{id:long}")]
    [AdminOrTeacher]
    public async Task<ActionResult<QuizQuestion>> Get(long id, [FromServices] Auth auth)
    {
        var question = await _quizQuestionService.Get(auth, id);
        if (question is null)
        {
            return NotFound(new { Message = "Question not found" });
        }
        return Ok(question);
    }

    /// <summary>
    /// Updates a quiz question. Admin and Teacher can update questions for quizzes they own.
    /// </summary>
    [HttpPut("{id:long}")]
    [AdminOrTeacher]
    public async Task<ActionResult<QuizQuestion>> Update(
        long id,
        [FromBody] UpdateQuizQuestionRequest request,
        [FromServices] Auth auth)
    {
        var question = await _quizQuestionService.Update(auth, new QuizQuestion.Update(
            id,
            request.Prompt,
            request.TypeId,
            request.Properties,
            request.Score,
            request.Order));

        return Ok(question);
    }

    /// <summary>
    /// Deactivates a quiz question (soft delete). Admin and Teacher can deactivate questions for quizzes they own.
    /// </summary>
    [HttpPost("{id:long}/deactivate")]
    [AdminOrTeacher]
    public async Task<ActionResult> Deactivate(long id, [FromServices] Auth auth)
    {
        await _quizQuestionService.Deactivate(auth, id);
        return Ok(new { Message = "Question deactivated successfully" });
    }

    /// <summary>
    /// Reactivates a previously deactivated quiz question.
    /// </summary>
    [HttpPost("{id:long}/reactivate")]
    [AdminOrTeacher]
    public async Task<ActionResult> Reactivate(long id, [FromServices] Auth auth)
    {
        await _quizQuestionService.Reactivate(auth, id);
        return Ok(new { Message = "Question reactivated successfully" });
    }

    /// <summary>
    /// Permanently deletes a quiz question. Admin only.
    /// </summary>
    [HttpDelete("{id:long}")]
    [AdminOnly]
    public async Task<ActionResult> Delete(long id, [FromServices] Auth auth)
    {
        await _quizQuestionService.Delete(auth, id);
        return NoContent();
    }

    /// <summary>
    /// Searches for quiz questions.
    /// </summary>
    [HttpGet]
    [AdminOrTeacher]
    public async Task<ActionResult<Paginated<QuizQuestion>>> Search(
        [FromQuery] long? quizId,
        [FromQuery] long? typeId,
        [FromQuery] bool? isActive,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] bool includeQuiz = false,
        [FromQuery] bool includeType = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromServices] Auth auth = null!)
    {
        var filter = new QuizQuestion.Filter(
            QuizId: quizId,
            TypeId: typeId,
            IsActive: isActive,
            DateRange: startDate.HasValue || endDate.HasValue
                ? new DateTimeRange(startDate, endDate)
                : null,
            IncludeQuiz: includeQuiz ? true : null,
            IncludeType: includeType ? true : null);

        var pagination = new PaginationOptions(page, pageSize);
        var result = await _quizQuestionService.Search(auth, filter, pagination);

        return Ok(result);
    }
}

/// <summary>
/// Request model for updating a quiz question.
/// </summary>
public record UpdateQuizQuestionRequest(
    string? Prompt = null,
    long? TypeId = null,
    string? Properties = null,
    int? Score = null,
    int? Order = null
);

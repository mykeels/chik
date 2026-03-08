using Microsoft.AspNetCore.Mvc;

namespace Chik.Exams.Api;

[ApiController]
[Route("api/quizzes")]
public class QuizzesController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly IQuizQuestionService _quizQuestionService;
    private readonly ILogger<QuizzesController> _logger;

    public QuizzesController(
        IQuizService quizService,
        IQuizQuestionService quizQuestionService,
        ILogger<QuizzesController> logger)
    {
        _quizService = quizService;
        _quizQuestionService = quizQuestionService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new quiz. Admin and Teacher can create quizzes.
    /// When an admin creates a quiz, they can assign an examiner (teacher) who will be responsible for the quiz.
    /// </summary>
    [HttpPost]
    [AdminOrTeacher]
    public async Task<ActionResult<Quiz>> Create(
        [FromBody] CreateQuizRequest request,
        [FromServices] Auth auth)
    {
        var quiz = await _quizService.Create(auth, new Quiz.Create(
            request.Title,
            request.Description,
            auth.Id,
            request.ExaminerId,
            request.Duration));

        _logger.LogInformation("Quiz '{Title}' created by {Creator}", quiz.Title, auth.Username);
        return CreatedAtAction(nameof(Get), new { id = quiz.Id }, quiz);
    }

    /// <summary>
    /// Gets a quiz by ID. Admin can see all, Teacher can see their own or where they are examiner.
    /// </summary>
    [HttpGet("{id:long}")]
    [AdminOrTeacher]
    public async Task<ActionResult<Quiz>> Get(
        long id,
        [FromQuery] bool includeQuestions = false,
        [FromServices] Auth auth = null!)
    {
        var quiz = await _quizService.Get(auth, id, includeQuestions);
        if (quiz is null)
        {
            return NotFound(new { Message = "Quiz not found" });
        }
        return Ok(quiz);
    }

    /// <summary>
    /// Updates a quiz. Admin can update any, Teacher can only update their own.
    /// </summary>
    [HttpPut("{id:long}")]
    [AdminOrTeacher]
    public async Task<ActionResult<Quiz>> Update(
        long id,
        [FromBody] UpdateQuizRequest request,
        [FromServices] Auth auth)
    {
        var quiz = await _quizService.Update(auth, new Quiz.Update(
            id,
            request.Title,
            request.Description,
            request.ExaminerId,
            request.Duration));

        return Ok(quiz);
    }

    /// <summary>
    /// Deletes a quiz. Admin can delete any, Teacher can only delete their own.
    /// </summary>
    [HttpDelete("{id:long}")]
    [AdminOrTeacher]
    public async Task<ActionResult> Delete(long id, [FromServices] Auth auth)
    {
        await _quizService.Delete(auth, id);
        return NoContent();
    }

    /// <summary>
    /// Searches for quizzes. Admin sees all, Teacher sees their own or where they are examiner.
    /// </summary>
    [HttpGet]
    [AdminOrTeacher]
    public async Task<ActionResult<Paginated<Quiz>>> Search(
        [FromServices] Auth auth,
        [FromQuery] Quiz.Filter? filter,
        [FromQuery] PaginationOptions? pagination)
    {
        var result = await _quizService.Search(auth, filter, pagination);

        return Ok(result);
    }

    #region Quiz Questions

    /// <summary>
    /// Gets all questions for a quiz.
    /// </summary>
    [HttpGet("{quizId:long}/questions")]
    [AdminOrTeacher]
    public async Task<ActionResult<List<QuizQuestion>>> GetQuestions(
        long quizId,
        [FromQuery] bool includeDeactivated = false,
        [FromServices] Auth auth = null!)
    {
        var questions = await _quizQuestionService.GetByQuizId(auth, quizId, includeDeactivated);
        return Ok(questions);
    }

    /// <summary>
    /// Creates a new question for a quiz. Admin and Teacher can create questions for quizzes they own.
    /// </summary>
    [HttpPost("{quizId:long}/questions")]
    [AdminOrTeacher]
    public async Task<ActionResult<QuizQuestion>> CreateQuestion(
        long quizId,
        [FromBody] CreateQuizQuestionRequest request,
        [FromServices] Auth auth)
    {
        var question = await _quizQuestionService.Create(auth, new QuizQuestion.Create(
            quizId,
            request.Prompt,
            request.TypeId,
            request.Properties,
            request.Score,
            request.Order));

        return CreatedAtAction(
            nameof(QuizQuestionsController.Get),
            "QuizQuestions",
            new { id = question.Id },
            question);
    }

    /// <summary>
    /// Reorders questions within a quiz.
    /// </summary>
    [HttpPost("{quizId:long}/questions/reorder")]
    [AdminOrTeacher]
    public async Task<ActionResult> ReorderQuestions(
        long quizId,
        [FromBody] ReorderQuestionsRequest request,
        [FromServices] Auth auth)
    {
        await _quizQuestionService.ReorderQuestions(auth, quizId, request.QuestionIdsInOrder);
        return Ok(new { Message = "Questions reordered successfully" });
    }

    #endregion
}

/// <summary>
/// Request model for creating a quiz.
/// </summary>
public record CreateQuizRequest(
    [Required] string Title,
    [Required] string Description,
    long? ExaminerId = null,
    TimeSpan? Duration = null
);

/// <summary>
/// Request model for updating a quiz.
/// </summary>
public record UpdateQuizRequest(
    string? Title = null,
    string? Description = null,
    long? ExaminerId = null,
    TimeSpan? Duration = null
);

/// <summary>
/// Request model for creating a quiz question.
/// </summary>
public record CreateQuizQuestionRequest(
    [Required] string Prompt,
    [Required] long TypeId,
    [Required] string Properties,
    [Required] int Score,
    int Order = 0
);

/// <summary>
/// Request model for reordering quiz questions.
/// </summary>
public record ReorderQuestionsRequest(
    [Required] List<long> QuestionIdsInOrder
);

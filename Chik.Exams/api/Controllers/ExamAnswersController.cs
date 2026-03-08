using Microsoft.AspNetCore.Mvc;

namespace Chik.Exams.Api;

[ApiController]
[Route("api/exam-answers")]
public class ExamAnswersController : ControllerBase
{
    private readonly IExamAnswerService _examAnswerService;
    private readonly ILogger<ExamAnswersController> _logger;

    public ExamAnswersController(
        IExamAnswerService examAnswerService,
        ILogger<ExamAnswersController> logger)
    {
        _examAnswerService = examAnswerService;
        _logger = logger;
    }

    /// <summary>
    /// Gets an exam answer by ID.
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ExamAnswer>> Get(long id, [FromServices] Auth auth)
    {
        var answer = await _examAnswerService.Get(auth, id);
        if (answer is null)
        {
            return NotFound(new { Message = "Answer not found" });
        }
        return Ok(answer);
    }

    /// <summary>
    /// Updates an exam answer. Only the student taking the exam can update before submission.
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ExamAnswer>> Update(
        long id,
        [FromBody] UpdateExamAnswerRequest request,
        [FromServices] Auth auth)
    {
        var answer = await _examAnswerService.Update(auth, new ExamAnswer.Update(
            id,
            request.Answer));

        return Ok(answer);
    }

    /// <summary>
    /// Scores an answer manually (examiner score). Admin and Teacher can score.
    /// Examiner-scored answers override auto-scored answers.
    /// </summary>
    [HttpPost("{id:long}/score")]
    [AdminOrTeacher]
    public async Task<ActionResult<ExamAnswer>> ExaminerScore(
        long id,
        [FromBody] ScoreAnswerRequest request,
        [FromServices] Auth auth)
    {
        var answer = await _examAnswerService.ExaminerScore(auth, id, request.Score, request.Comment);
        _logger.LogInformation("Answer {AnswerId} scored by {Examiner} with score {Score}", id, auth.Username, request.Score);
        return Ok(answer);
    }

    /// <summary>
    /// Searches for exam answers.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Paginated<ExamAnswer>>> Search(
        [FromQuery] long? examId,
        [FromQuery] long? questionId,
        [FromQuery] long? examinerId,
        [FromQuery] bool? isAutoScored,
        [FromQuery] bool? isExaminerScored,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] bool includeExam = false,
        [FromQuery] bool includeQuestion = false,
        [FromQuery] bool includeExaminer = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromServices] Auth auth = null!)
    {
        var filter = new ExamAnswer.Filter(
            ExamId: examId,
            QuestionId: questionId,
            ExaminerId: examinerId,
            IsAutoScored: isAutoScored,
            IsExaminerScored: isExaminerScored,
            DateRange: startDate.HasValue || endDate.HasValue
                ? DateTimeRange.Between(startDate, endDate)
                : null,
            IncludeExam: includeExam ? true : null,
            IncludeQuestion: includeQuestion ? true : null,
            IncludeExaminer: includeExaminer ? true : null);

        var pagination = new PaginationOptions(page, pageSize);
        var result = await _examAnswerService.Search(auth, filter, pagination);

        return Ok(result);
    }
}

/// <summary>
/// Request model for updating an exam answer.
/// </summary>
public record UpdateExamAnswerRequest(
    string? Answer = null
);

/// <summary>
/// Request model for scoring an answer.
/// </summary>
public record ScoreAnswerRequest(
    [Required] int Score,
    string? Comment = null
);

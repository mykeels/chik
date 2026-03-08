using Microsoft.AspNetCore.Mvc;

namespace Chik.Exams.Api;

[ApiController]
[Route("api/exams")]
public class ExamsController : ControllerBase
{
    private readonly IExamService _examService;
    private readonly IExamAnswerService _examAnswerService;
    private readonly ILogger<ExamsController> _logger;

    public ExamsController(
        IExamService examService,
        IExamAnswerService examAnswerService,
        ILogger<ExamsController> logger)
    {
        _examService = examService;
        _examAnswerService = examAnswerService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new exam (assigns a quiz to a student). Admin and Teacher can create exams.
    /// </summary>
    [HttpPost]
    [AdminOrTeacher]
    public async Task<ActionResult<Exam>> Create(
        [FromBody] CreateExamRequest request,
        [FromServices] Auth auth)
    {
        try
        {
            var exam = await _examService.Create(auth, new Exam.Create(
                request.UserId,
                request.QuizId,
                auth.Id));
            
            _logger.LogInformation("Exam created for user {UserId} by {Creator}", request.UserId, auth.Username);
            return CreatedAtAction(nameof(Get), new { id = exam.Id }, exam);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Gets an exam by ID. Admin sees all, Teacher sees their created exams, Student sees their own exams.
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<Exam>> Get(
        long id,
        [FromQuery] bool includeAnswers = false,
        [FromServices] Auth auth = null!)
    {
        try
        {
            var exam = await _examService.Get(auth, id, includeAnswers);
            if (exam is null)
            {
                return NotFound(new { Message = "Exam not found" });
            }
            return Ok(exam);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Updates an exam. Admin can update any, Teacher can update their created exams.
    /// </summary>
    [HttpPut("{id:long}")]
    [AdminOrTeacher]
    public async Task<ActionResult<Exam>> Update(
        long id,
        [FromBody] UpdateExamRequest request,
        [FromServices] Auth auth)
    {
        try
        {
            var exam = await _examService.Update(auth, new Exam.Update(
                id,
                request.StartedAt,
                request.EndedAt,
                request.Score,
                request.ExaminerId,
                request.ExaminerComment));
            
            return Ok(exam);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Message = "Exam not found" });
        }
    }

    /// <summary>
    /// Starts an exam (sets StartedAt). Only the assigned student can start.
    /// </summary>
    [HttpPost("{id:long}/start")]
    public async Task<ActionResult<Exam>> Start(long id, [FromServices] Auth auth)
    {
        try
        {
            var exam = await _examService.Start(auth, id);
            _logger.LogInformation("Exam {ExamId} started by {User}", id, auth.Username);
            return Ok(exam);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Message = "Exam not found" });
        }
    }

    /// <summary>
    /// Ends/submits an exam (sets EndedAt). Only the assigned student can end.
    /// </summary>
    [HttpPost("{id:long}/submit")]
    public async Task<ActionResult<Exam>> Submit(long id, [FromServices] Auth auth)
    {
        try
        {
            var exam = await _examService.End(auth, id);
            _logger.LogInformation("Exam {ExamId} submitted by {User}", id, auth.Username);
            return Ok(exam);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Message = "Exam not found" });
        }
    }

    /// <summary>
    /// Marks an exam with a score and optional comment. Admin and Teacher (creator/examiner) can mark.
    /// </summary>
    [HttpPost("{id:long}/mark")]
    [AdminOrTeacher]
    public async Task<ActionResult<Exam>> Mark(
        long id,
        [FromBody] MarkExamRequest request,
        [FromServices] Auth auth)
    {
        try
        {
            var exam = await _examService.Mark(auth, id, request.Score, request.Comment);
            _logger.LogInformation("Exam {ExamId} marked by {Examiner} with score {Score}", id, auth.Username, request.Score);
            return Ok(exam);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Message = "Exam not found" });
        }
    }

    /// <summary>
    /// Auto-scores all answers in an exam where possible.
    /// </summary>
    [HttpPost("{id:long}/auto-score")]
    [AdminOrTeacher]
    public async Task<ActionResult> AutoScore(long id, [FromServices] Auth auth)
    {
        try
        {
            await _examService.AutoScore(auth, id);
            return Ok(new { Message = "Auto-scoring completed" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Message = "Exam not found" });
        }
    }

    /// <summary>
    /// Gets the scores for an exam, aggregating auto-scored and examiner-scored answers.
    /// Examiner-scored answers override auto-scored answers.
    /// </summary>
    [HttpGet("{id:long}/scores")]
    public async Task<ActionResult<ExamScores>> GetScores(long id, [FromServices] Auth auth)
    {
        try
        {
            var scores = await _examService.GetScores(auth, id);
            return Ok(scores);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Message = "Exam not found" });
        }
    }

    /// <summary>
    /// Cancels an exam. Admin can cancel any, Teacher can cancel their created exams.
    /// </summary>
    [HttpPost("{id:long}/cancel")]
    [AdminOrTeacher]
    public async Task<ActionResult> Cancel(long id, [FromServices] Auth auth)
    {
        try
        {
            await _examService.Cancel(auth, id);
            return Ok(new { Message = "Exam cancelled successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Message = "Exam not found" });
        }
    }

    /// <summary>
    /// Deletes an exam. Admin only.
    /// </summary>
    [HttpDelete("{id:long}")]
    [AdminOnly]
    public async Task<ActionResult> Delete(long id, [FromServices] Auth auth)
    {
        try
        {
            await _examService.Delete(auth, id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Message = "Exam not found" });
        }
    }

    /// <summary>
    /// Gets pending exams for a student. Admin/Teacher can view any student's pending exams.
    /// Students can only view their own.
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<Exam>>> GetPendingExams(
        [FromQuery] long? studentId,
        [FromServices] Auth auth)
    {
        try
        {
            var exams = await _examService.GetPendingExams(auth, studentId);
            return Ok(exams);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Gets exam history for a student. Admin/Teacher can view any student's history.
    /// Students can only view their own.
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<List<Exam>>> GetExamHistory(
        [FromQuery] long? studentId,
        [FromServices] Auth auth)
    {
        try
        {
            var exams = await _examService.GetExamHistory(auth, studentId);
            return Ok(exams);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Searches for exams.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<Paginated<Exam>>> Search(
        [FromQuery] long? userId,
        [FromQuery] long? quizId,
        [FromQuery] long? creatorId,
        [FromQuery] long? examinerId,
        [FromQuery] bool? isStarted,
        [FromQuery] bool? isEnded,
        [FromQuery] bool? isMarked,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] bool includeUser = false,
        [FromQuery] bool includeQuiz = false,
        [FromQuery] bool includeCreator = false,
        [FromQuery] bool includeExaminer = false,
        [FromQuery] bool includeAnswers = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromServices] Auth auth = null!)
    {
        try
        {
            var filter = new Exam.Filter(
                UserId: userId,
                QuizId: quizId,
                CreatorId: creatorId,
                ExaminerId: examinerId,
                IsStarted: isStarted,
                IsEnded: isEnded,
                IsMarked: isMarked,
                DateRange: startDate.HasValue || endDate.HasValue 
                    ? new DateTimeRange(startDate, endDate) 
                    : null,
                IncludeUser: includeUser ? true : null,
                IncludeQuiz: includeQuiz ? true : null,
                IncludeCreator: includeCreator ? true : null,
                IncludeExaminer: includeExaminer ? true : null,
                IncludeAnswers: includeAnswers ? true : null);
            
            var pagination = new PaginationOptions(page, pageSize);
            var result = await _examService.Search(auth, filter, pagination);
            
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    #region Exam Answers

    /// <summary>
    /// Gets all answers for an exam.
    /// </summary>
    [HttpGet("{examId:long}/answers")]
    public async Task<ActionResult<List<ExamAnswer>>> GetAnswers(
        long examId,
        [FromServices] Auth auth)
    {
        try
        {
            var answers = await _examAnswerService.GetByExamId(auth, examId);
            return Ok(answers);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Submits an answer for a question in an exam. Only the student taking the exam can submit.
    /// </summary>
    [HttpPost("{examId:long}/answers")]
    public async Task<ActionResult<ExamAnswer>> SubmitAnswer(
        long examId,
        [FromBody] SubmitAnswerRequest request,
        [FromServices] Auth auth)
    {
        try
        {
            var answer = await _examAnswerService.SubmitAnswer(auth, examId, request.QuestionId, request.Answer);
            return Ok(answer);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    #endregion
}

/// <summary>
/// Request model for creating an exam.
/// </summary>
public record CreateExamRequest(
    [Required] long UserId,
    [Required] long QuizId
);

/// <summary>
/// Request model for updating an exam.
/// </summary>
public record UpdateExamRequest(
    DateTime? StartedAt = null,
    DateTime? EndedAt = null,
    int? Score = null,
    long? ExaminerId = null,
    string? ExaminerComment = null
);

/// <summary>
/// Request model for marking an exam.
/// </summary>
public record MarkExamRequest(
    [Required] int Score,
    string? Comment = null
);

/// <summary>
/// Request model for submitting an answer.
/// </summary>
public record SubmitAnswerRequest(
    [Required] long QuestionId,
    [Required] string Answer
);

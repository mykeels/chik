using Chik.Exams.Data;
using Newtonsoft.Json;

namespace Chik.Exams;

public interface IExamService
{
    public IExamRepository Repository { get; }

    /// <summary>
    /// Creates a new exam (assigns a quiz to a student). Admin and Teacher can create exams.
    /// </summary>
    Task<Exam> Create(Auth auth, Exam.Create exam);

    /// <summary>
    /// Assigns a quiz to every student in a class (skips students who already have that quiz assigned).
    /// </summary>
    Task<List<Exam>> AssignToClass(Auth auth, int classId, long quizId);

    /// <summary>
    /// Gets an exam by ID. Admin sees all, Teacher sees their created exams, Student sees their own exams.
    /// </summary>
    Task<Exam?> Get(Auth auth, long id, bool includeAnswers = false);

    /// <summary>
    /// Updates an exam. Admin can update any, Teacher can update their created exams.
    /// </summary>
    Task<Exam> Update(Auth auth, Exam.Update exam);

    /// <summary>
    /// Starts an exam (sets StartedAt). Only the assigned student can start.
    /// </summary>
    Task<Exam> Start(Auth auth, long id);

    /// <summary>
    /// Ends/submits an exam (sets EndedAt). Only the assigned student can end.
    /// </summary>
    Task<Exam> End(Auth auth, long id);

    /// <summary>
    /// Marks an exam with a score and optional comment. Admin and Teacher (creator/examiner) can mark.
    /// </summary>
    Task<Exam> Mark(Auth auth, long id, int score, string? comment = null);

    /// <summary>
    /// Auto-scores all answers in an exam where possible.
    /// </summary>
    Task AutoScore(Auth auth, long examId);

    /// <summary>
    /// Gets the scores for an exam, aggregating auto-scored and examiner-scored answers.
    /// </summary>
    Task<ExamScores> GetScores(Auth auth, long examId);

    /// <summary>
    /// Cancels an exam. Admin can cancel any, Teacher can cancel their created exams.
    /// </summary>
    Task Cancel(Auth auth, long id);

    /// <summary>
    /// Deletes an exam. Admin only.
    /// </summary>
    Task Delete(Auth auth, long id);

    /// <summary>
    /// Gets pending exams for a student.
    /// </summary>
    Task<List<Exam>> GetPendingExams(Auth auth, long? studentId = null);

    /// <summary>
    /// Gets exam history for a student.
    /// </summary>
    Task<List<Exam>> GetExamHistory(Auth auth, long? studentId = null);

    /// <summary>
    /// Searches for exams.
    /// </summary>
    Task<Paginated<Exam>> Search(Auth auth, Exam.Filter? filter = null, PaginationOptions? pagination = null);
}

/// <summary>
/// Represents the scores for an exam, including individual answer scores.
/// </summary>
public record ExamScores(
    long ExamId,
    int TotalScore,
    int MaxPossibleScore,
    int AnsweredQuestions,
    int TotalQuestions,
    List<AnswerScore> AnswerScores
);

/// <summary>
/// Represents the score for a single answer.
/// </summary>
public record AnswerScore(
    long QuestionId,
    [property: JsonProperty("autoScore", NullValueHandling = NullValueHandling.Include)]
    int? AutoScore,
    [property: JsonProperty("examinerScore", NullValueHandling = NullValueHandling.Include)]
    int? ExaminerScore,
    int FinalScore,
    int MaxScore
);

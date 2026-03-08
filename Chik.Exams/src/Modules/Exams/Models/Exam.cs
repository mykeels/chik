namespace Chik.Exams;

public record Exam(
    long Id,
    long UserId,
    long QuizId,
    long CreatorId,
    DateTime? StartedAt,
    DateTime? EndedAt,
    int? Score,
    long? ExaminerId,
    string? ExaminerComment,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public User? User { get; set; }
    public Quiz? Quiz { get; set; }
    public User? Creator { get; set; }
    public User? Examiner { get; set; }
    public List<ExamAnswer>? Answers { get; set; }

    public bool IsStarted => StartedAt is not null;
    public bool IsEnded => EndedAt is not null;
    public bool IsMarked => Score is not null;

    public record Create(
        long UserId,
        long QuizId,
        long CreatorId
    );

    public record Update(
        long Id,
        DateTime? StartedAt = null,
        DateTime? EndedAt = null,
        int? Score = null,
        long? ExaminerId = null,
        string? ExaminerComment = null
    );

    public record Filter(
        long? UserId = null,
        long? QuizId = null,
        long? CreatorId = null,
        long? ExaminerId = null,
        bool? IsStarted = null,
        bool? IsEnded = null,
        bool? IsMarked = null,
        DateTimeRange? DateRange = null,
        List<long>? ExamIds = null,
        bool? IncludeUser = null,
        bool? IncludeQuiz = null,
        bool? IncludeCreator = null,
        bool? IncludeExaminer = null,
        bool? IncludeAnswers = null
    );
}

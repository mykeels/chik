namespace Chik.Exams;

public record ExamAnswer(
    long Id,
    long ExamId,
    long QuestionId,
    string Answer,
    int? AutoScore,
    int? ExaminerScore,
    long? ExaminerId,
    string? ExaminerComment,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public Exam? Exam { get; set; }
    public QuizQuestion? Question { get; set; }
    public User? Examiner { get; set; }

    public int? FinalScore => ExaminerScore ?? AutoScore;

    public record Create(
        long ExamId,
        long QuestionId,
        string Answer
    );

    public record Update(
        long Id,
        string? Answer = null,
        int? AutoScore = null,
        int? ExaminerScore = null,
        long? ExaminerId = null,
        string? ExaminerComment = null
    );

    public record Filter(
        long? ExamId = null,
        long? QuestionId = null,
        long? ExaminerId = null,
        bool? IsAutoScored = null,
        bool? IsExaminerScored = null,
        DateTimeRange? DateRange = null,
        List<long>? AnswerIds = null,
        bool? IncludeExam = null,
        bool? IncludeQuestion = null,
        bool? IncludeExaminer = null
    );
}

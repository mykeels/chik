namespace Chik.Exams.Data;

public class ExamAnswerDbo
{
    public long Id { get; set; }
    public long ExamId { get; set; }
    public long QuestionId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public int? AutoScore { get; set; }
    public int? ExaminerScore { get; set; }
    public long? ExaminerId { get; set; }
    public string? ExaminerComment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ExamDbo? Exam { get; set; }
    public virtual QuizQuestionDbo? Question { get; set; }
    public virtual UserDbo? Examiner { get; set; }

    public static implicit operator ExamAnswerDbo(ExamAnswer answer) => new()
    {
        Id = answer.Id,
        ExamId = answer.ExamId,
        QuestionId = answer.QuestionId,
        Answer = answer.Answer,
        AutoScore = answer.AutoScore,
        ExaminerScore = answer.ExaminerScore,
        ExaminerId = answer.ExaminerId,
        ExaminerComment = answer.ExaminerComment,
        CreatedAt = answer.CreatedAt,
        UpdatedAt = answer.UpdatedAt
    };

    public ExamAnswer ToModel() => new(
        Id,
        ExamId,
        QuestionId,
        Answer,
        AutoScore,
        ExaminerScore,
        ExaminerId,
        ExaminerComment,
        CreatedAt,
        UpdatedAt
    )
    {
        Exam = Exam?.ToModel(),
        Question = Question?.ToModel(),
        Examiner = Examiner?.ToModel()
    };
}

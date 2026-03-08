namespace Chik.Exams.Data;

public class ExamDbo
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long QuizId { get; set; }
    public long CreatorId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? Score { get; set; }
    public long? ExaminerId { get; set; }
    public string? ExaminerComment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual UserDbo? User { get; set; }
    public virtual QuizDbo? Quiz { get; set; }
    public virtual UserDbo? Creator { get; set; }
    public virtual UserDbo? Examiner { get; set; }
    public virtual List<ExamAnswerDbo>? Answers { get; set; }

    public static implicit operator ExamDbo(Exam exam) => new()
    {
        Id = exam.Id,
        UserId = exam.UserId,
        QuizId = exam.QuizId,
        CreatorId = exam.CreatorId,
        StartedAt = exam.StartedAt,
        EndedAt = exam.EndedAt,
        Score = exam.Score,
        ExaminerId = exam.ExaminerId,
        ExaminerComment = exam.ExaminerComment,
        CreatedAt = exam.CreatedAt,
        UpdatedAt = exam.UpdatedAt
    };

    public static implicit operator Exam?(ExamDbo? dbo) => dbo is null ? null : new(
        dbo.Id,
        dbo.UserId,
        dbo.QuizId,
        dbo.CreatorId,
        dbo.StartedAt,
        dbo.EndedAt,
        dbo.Score,
        dbo.ExaminerId,
        dbo.ExaminerComment,
        dbo.CreatedAt,
        dbo.UpdatedAt
    )
    {
        User = dbo.User,
        Quiz = dbo.Quiz,
        Creator = dbo.Creator,
        Examiner = dbo.Examiner,
        Answers = dbo.Answers?.Select(a => (ExamAnswer?)a).Where(a => a != null).Cast<ExamAnswer>().ToList()
    };
}

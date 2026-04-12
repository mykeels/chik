namespace Chik.Exams.Data;

public class ExamDbo
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long QuizId { get; set; }
    public long CreatorId { get; set; }
    public int StudentClassId { get; set; }
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
    public virtual ClassDbo? StudentClass { get; set; }
    public virtual UserDbo? Creator { get; set; }
    public virtual UserDbo? Examiner { get; set; }
    public virtual List<ExamAnswerDbo>? Answers { get; set; }

    public static implicit operator ExamDbo(Exam exam) => new()
    {
        Id = exam.Id,
        UserId = exam.UserId,
        QuizId = exam.QuizId,
        CreatorId = exam.CreatorId,
        StudentClassId = exam.StudentClassId,
        StartedAt = exam.StartedAt,
        EndedAt = exam.EndedAt,
        Score = exam.Score,
        ExaminerId = exam.ExaminerId,
        ExaminerComment = exam.ExaminerComment,
        CreatedAt = exam.CreatedAt,
        UpdatedAt = exam.UpdatedAt
    };

    public Exam ToModel() => new(
        Id,
        UserId,
        QuizId,
        CreatorId,
        StudentClassId,
        StartedAt,
        EndedAt,
        Score,
        ExaminerId,
        ExaminerComment,
        CreatedAt,
        UpdatedAt
    )
    {
        User = User?.ToModel(),
        Quiz = Quiz?.ToModel(),
        StudentClass = StudentClass?.ToModel(),
        Creator = Creator?.ToModel(),
        Examiner = Examiner?.ToModel(),
        Answers = Answers?.Select(a => a.ToModel()).ToList()
    };
}

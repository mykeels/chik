namespace Chik.Exams.Data;

public class UserDbo
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Roles { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual List<QuizDbo>? CreatedQuizzes { get; set; }
    public virtual List<ExamDbo>? CreatedExams { get; set; }
    public virtual List<ExamDbo>? TakenExams { get; set; }
    public virtual List<ExamDbo>? ExaminedExams { get; set; }
    public virtual List<ExamAnswerDbo>? ExaminedAnswers { get; set; }
    public virtual List<LoginDbo>? Logins { get; set; }

    public static implicit operator UserDbo(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Roles = user.Roles.ToInt32(),
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };

    public static implicit operator User?(UserDbo? dbo) => dbo is null ? null : new(
        dbo.Id,
        dbo.Username,
        UserRoleExtensions.FromInt32(dbo.Roles),
        dbo.CreatedAt,
        dbo.UpdatedAt
    );
}

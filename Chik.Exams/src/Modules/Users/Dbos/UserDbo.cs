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
    public virtual List<QuizDbo>? ExaminedQuizzes { get; set; }
    public virtual List<ExamDbo>? CreatedExams { get; set; }
    public virtual List<ExamDbo>? TakenExams { get; set; }
    public virtual List<ExamDbo>? ExaminedExams { get; set; }
    public virtual List<ExamAnswerDbo>? ExaminedAnswers { get; set; }
    public virtual List<LoginDbo>? Logins { get; set; }
    /// <summary>Class membership: one row for students (enforced in app), many for teachers.</summary>
    public virtual List<UserClassDbo>? UserClasses { get; set; }

    public static implicit operator UserDbo(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Roles = user.Roles.ToInt32(),
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };

    public User ToModel()
    {
        var user = new User(Id, Username, Roles.ToEnumList<UserRole>(), CreatedAt, UpdatedAt);
        if ((Roles & (int)UserRole.Student) != 0 && UserClasses is { Count: > 0 })
        {
            var uc = UserClasses.OrderBy(x => x.Id).First();
            if (uc.Class is not null)
                user.Student = new Student(uc.Class.ToModel());
        }
        if ((Roles & (int)UserRole.Teacher) != 0 && UserClasses is { Count: > 0 })
            user.Teacher = new Teacher(UserClasses.Where(uc => uc.Class is not null).Select(uc => uc.Class!.ToModel()).ToList());
        return user;
    }
}

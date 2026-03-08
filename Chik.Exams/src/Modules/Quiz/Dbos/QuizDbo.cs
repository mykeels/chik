namespace Chik.Exams.Data;

public class QuizDbo
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long CreatorId { get; set; }
    public long? ExaminerId { get; set; }
    public TimeSpan? Duration { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual UserDbo? Creator { get; set; }
    public virtual UserDbo? Examiner { get; set; }
    public virtual List<QuizQuestionDbo>? Questions { get; set; }
    public virtual List<ExamDbo>? Exams { get; set; }

    public static implicit operator QuizDbo(Quiz quiz) => new()
    {
        Id = quiz.Id,
        Title = quiz.Title,
        Description = quiz.Description,
        CreatorId = quiz.CreatorId,
        ExaminerId = quiz.ExaminerId,
        Duration = quiz.Duration,
        CreatedAt = quiz.CreatedAt,
        UpdatedAt = quiz.UpdatedAt
    };

    public Quiz ToModel() => new(
        Id,
        Title,
        Description,
        CreatorId,
        ExaminerId,
        Duration,
        CreatedAt,
        UpdatedAt
    );
}

namespace Chik.Exams.Data;

public class ExamDbo
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long QuizId { get; set; }
    public long CreatorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
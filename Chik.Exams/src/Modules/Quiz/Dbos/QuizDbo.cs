namespace Chik.Exams.Data;

public class QuizDbo
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public long CreatorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public virtual UserDbo? Creator { get; set; }
    public virtual List<QuizQuestionDbo>? QuizQuestions { get; set; }
}
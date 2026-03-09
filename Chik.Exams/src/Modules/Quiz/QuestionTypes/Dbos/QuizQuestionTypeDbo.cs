namespace Chik.Exams.Data;

public class QuizQuestionTypeDbo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual List<QuizQuestionDbo>? Questions { get; set; }

    public static implicit operator QuizQuestionTypeDbo(QuizQuestionType type) => new()
    {
        Id = type.Id,
        Name = type.Name,
        Description = type.Description,
        CreatedAt = type.CreatedAt,
        UpdatedAt = type.UpdatedAt
    };

    public QuizQuestionType ToModel() => new(
        Id,
        Name,
        Description,
        CreatedAt,
        UpdatedAt
    );
}

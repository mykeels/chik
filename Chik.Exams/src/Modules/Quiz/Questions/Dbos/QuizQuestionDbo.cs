using Newtonsoft.Json;

namespace Chik.Exams.Data;

public class QuizQuestionDbo
{
    public long Id { get; set; }
    public long QuizId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public string Properties { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }

    // Navigation properties
    public virtual QuizDbo? Quiz { get; set; }
    public virtual QuizQuestionTypeDbo? Type { get; set; }
    public virtual List<ExamAnswerDbo>? ExamAnswers { get; set; }

    public static implicit operator QuizQuestionDbo(QuizQuestion question) => new()
    {
        Id = question.Id,
        QuizId = question.QuizId,
        Prompt = question.Prompt,
        TypeId = question.TypeId,
        Properties = SerializeProperties(question.Properties),
        Score = question.Score,
        Order = question.Order,
        CreatedAt = question.CreatedAt,
        UpdatedAt = question.UpdatedAt,
        DeactivatedAt = question.DeactivatedAt
    };

    public QuizQuestion ToModel() => new(
        Id,
        QuizId,
        Prompt,
        TypeId,
        DeserializeProperties(Properties),
        Score,
        Order,
        CreatedAt,
        UpdatedAt,
        DeactivatedAt
    )
    {
        Quiz = Quiz?.ToModel(),
        Type = Type?.ToModel()
    };

    public static string SerializeProperties(QuizQuestion.QuestionType? properties)
    {
        if (properties is null) return "{}";
        return JsonConvert.SerializeObject(properties);
    }

    public static QuizQuestion.QuestionType? DeserializeProperties(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonConvert.DeserializeObject<QuizQuestion.QuestionType>(json);
    }
}

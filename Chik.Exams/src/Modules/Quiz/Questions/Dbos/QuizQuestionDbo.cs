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

    public static implicit operator QuizQuestion?(QuizQuestionDbo? dbo) => dbo is null ? null : new(
        dbo.Id,
        dbo.QuizId,
        dbo.Prompt,
        dbo.TypeId,
        DeserializeProperties(dbo.Properties),
        dbo.Score,
        dbo.Order,
        dbo.CreatedAt,
        dbo.UpdatedAt,
        dbo.DeactivatedAt
    )
    {
        Quiz = dbo.Quiz,
        Type = dbo.Type
    };

    private static string SerializeProperties(QuizQuestion.QuestionType? properties)
    {
        if (properties is null) return "{}";
        return JsonConvert.SerializeObject(properties);
    }

    private static QuizQuestion.QuestionType? DeserializeProperties(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonConvert.DeserializeObject<QuizQuestion.QuestionType>(json);
    }
}

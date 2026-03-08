namespace Chik.Exams;

public record QuizQuestion(
    long Id,
    long QuizId,
    string Prompt,
    int TypeId,
    string Properties,
    int Score,
    int Order,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeactivatedAt
)
{
    public Quiz? Quiz { get; set; }
    public QuizQuestionType? Type { get; set; }

    public bool IsActive => DeactivatedAt is null;

    public record Create(
        long QuizId,
        string Prompt,
        long TypeId,
        string Properties,
        int Score,
        int Order
    );

    public record Update(
        long Id,
        string? Prompt = null,
        long? TypeId = null,
        string? Properties = null,
        int? Score = null,
        int? Order = null,
        DateTime? DeactivatedAt = null
    );

    public record Filter(
        long? QuizId = null,
        long? TypeId = null,
        bool? IsActive = null,
        DateTimeRange? DateRange = null,
        List<long>? QuestionIds = null,
        bool? IncludeQuiz = null,
        bool? IncludeType = null
    );
}

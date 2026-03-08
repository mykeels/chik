namespace Chik.Exams;

public record QuizQuestionType(
    long Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public List<QuizQuestion>? Questions { get; set; }

    // Predefined question type IDs
    public static class Types
    {
        public const long MultipleChoice = 1;
        public const long SingleChoice = 2;
        public const long FillInTheBlank = 3;
        public const long Essay = 4;
        public const long ShortAnswer = 5;
        public const long TrueOrFalse = 6;
    }

    public record Create(
        string Name,
        string Description
    );

    public record Update(
        long Id,
        string? Name = null,
        string? Description = null
    );

    public record Filter(
        string? Name = null,
        DateTimeRange? DateRange = null,
        List<long>? TypeIds = null,
        bool? IncludeQuestions = null
    );
}

namespace Chik.Exams;

public record QuizQuestionType(
    int Id,
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
        public const int MultipleChoice = 1;
        public const int SingleChoice = 2;
        public const int FillInTheBlank = 3;
        public const int Essay = 4;
        public const int ShortAnswer = 5;
        public const int TrueOrFalse = 6;
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

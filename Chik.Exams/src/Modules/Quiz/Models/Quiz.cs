namespace Chik.Exams;

public record Quiz(
    long Id,
    string Title,
    string Description,
    long CreatorId,
    TimeSpan? Duration,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public User? Creator { get; set; }
    public List<QuizQuestion>? Questions { get; set; }

    public record Create(
        string Title,
        string Description,
        long CreatorId,
        TimeSpan? Duration = null
    );

    public record Update(
        long Id,
        string? Title = null,
        string? Description = null,
        TimeSpan? Duration = null
    );

    public record Filter(
        string? Title = null,
        long? CreatorId = null,
        DateTimeRange? DateRange = null,
        List<long>? QuizIds = null,
        bool? IncludeCreator = null,
        bool? IncludeQuestions = null
    );
}

namespace Chik.Exams;

public record Quiz(
    long Id,
    string Title,
    string Description,
    long CreatorId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
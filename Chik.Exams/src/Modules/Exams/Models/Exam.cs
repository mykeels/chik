namespace Chik.Exams;

public record Exam(
    long Id,
    long UserId,
    long QuizId,
    long CreatorId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
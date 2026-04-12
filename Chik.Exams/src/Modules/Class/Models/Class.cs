namespace Chik.Exams;

public record Class(
    int Id,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public record Create(string Name);
}
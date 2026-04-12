namespace Chik.Exams.Data;

public class ClassDbo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual List<UserClassDbo>? UserClasses { get; set; }
    public virtual List<ExamDbo>? Exams { get; set; }

    public Class ToModel() => new(Id, Name, CreatedAt, UpdatedAt);
}

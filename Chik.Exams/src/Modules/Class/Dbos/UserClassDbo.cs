namespace Chik.Exams.Data;

/// <summary>User–class membership: one row per student (enforced in app), many per teacher.</summary>
public class UserClassDbo
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public int ClassId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual UserDbo? User { get; set; }
    public virtual ClassDbo? Class { get; set; }
}

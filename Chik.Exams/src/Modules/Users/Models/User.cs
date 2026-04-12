using System.Diagnostics.CodeAnalysis;

namespace Chik.Exams;

public record User(
    long Id,
    string Username,
    List<UserRole> Roles,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public DateTime? LastLogin { get; set; }
    public Student? Student { get; set; }
    public Teacher? Teacher { get; set; }
    public static List<UserRole> RolesOf(params UserRole[] roles)
    {
        return roles.ToList();
    }

    public bool HasRole(UserRole role) => this.Roles.Contains(role);

    public bool IsAdmin() => HasRole(UserRole.Admin);

    [MemberNotNullWhen(true, nameof(Teacher))]
    public bool IsTeacher() => HasRole(UserRole.Teacher);

    [MemberNotNullWhen(true, nameof(Student))]
    public bool IsStudent() => HasRole(UserRole.Student);

    public record Create(
        string Username,
        [property: Newtonsoft.Json.JsonIgnore] string Password,
        List<UserRole> Roles,
        int? ClassId = null,
        List<int>? ClassIds = null
    );

    public record Update(
        long Id,
        string? Username = null,
        string? Password = null,
        List<UserRole>? Roles = null,
        int? ClassId = null,
        List<int>? ClassIds = null
    );

    public record Filter(
        string? Username = null,
        int? Roles = null,
        DateTimeRange? DateRange = null,
        List<long>? UserIds = null
    );

    public static Auth Admin => new(
        Id: 1,
        Username: "admin",
        Roles: [UserRole.Admin],
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: DateTime.UtcNow
    );

    public string Email = $"{Username}@chik.ng";
}

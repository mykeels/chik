namespace Chik.Exams;

public record User(
    long Id,
    string Username,
    string Password,
    int Roles,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public List<UserRole> GetRoles()
    {
        var roles = new List<UserRole>();
        foreach (UserRole role in Enum.GetValues<UserRole>())
        {
            if ((Roles & (int)role) == (int)role)
            {
                roles.Add(role);
            }
        }
        return roles;
    }

    public bool HasRole(UserRole role) => (Roles & (int)role) == (int)role;

    public bool IsAdmin() => HasRole(UserRole.Admin);

    public bool IsTeacher() => HasRole(UserRole.Teacher);

    public bool IsStudent() => HasRole(UserRole.Student);

    public record Create(
        string Username,
        string Password,
        int Roles
    );

    public record Update(
        long Id,
        string? Username = null,
        string? Password = null,
        int? Roles = null
    );

    public record Filter(
        string? Username = null,
        int? Roles = null,
        DateTimeRange? DateRange = null,
        List<long>? UserIds = null
    );
}

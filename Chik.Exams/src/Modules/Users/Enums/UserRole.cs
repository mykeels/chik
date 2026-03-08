namespace Chik.Exams;

public enum UserRole
{
    Admin = 1,
    Teacher = 2,
    Student = 4
}

public static class UserRoleExtensions
{
    public static int ToInt32(this List<UserRole> roles)
    {
        // use bitmask to convert the roles to an integer
        int result = 0;
        foreach (var role in roles)
        {
            result |= (int)role;
        }
        return result;
    }

    public static List<UserRole> FromInt32(int value)
    {
        return Enum.GetValues<UserRole>().Where(role => (value & (int)role) == (int)role).ToList();
    }
}
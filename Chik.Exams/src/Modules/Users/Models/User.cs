using System.Text.RegularExpressions;

namespace Chik.Exams;

public record User(
    long Id,
    string UserName,
    List<UserRole> Roles,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public Login? LastLogin { get; set; }
    public static Auth Admin => new Auth(
        Guid.Empty,
        "admin@mykeels.com",
        "Admin",
        DateOnly.FromDateTime(DateTime.UtcNow), 
        DateTime.UtcNow,
        true,
        DateTime.UtcNow
    );
    
    public bool IsAdmin()
    {
        string[] adminEmails = ["admin@mykeels.com"];
        return adminEmails.Contains(this.Email);
    }

    public bool IsInternal()
    {
        bool isInternal = this.IsAdmin();
        var mykehell123 = new Regex(@"^mykehell123(\+.+)?@gmail\.com$");
        var graceama56 = new Regex(@"^graceama56(\+.+)?@gmail\.com$");
        var mikechifamily = new Regex(@"^mikechifamily(\+.+)?@gmail\.com$");
        var michaelikechim = new Regex(@"^michaelikechim(\+.+)?@gmail\.com$");
        var under4games = new Regex(@"@under4\.games$");
        isInternal |= mykehell123.IsMatch(this.Email);
        isInternal |= graceama56.IsMatch(this.Email);
        isInternal |= mikechifamily.IsMatch(this.Email);
        isInternal |= michaelikechim.IsMatch(this.Email);
        isInternal |= under4games.IsMatch(this.Email) && this.Email.Contains("internal");
        return isInternal;
    }

    public static async Task<Auth> FromUserName(string userName)
    {
        var userService = Provider.GetRequiredService<IUserService>();
        var user = await userService.Get(new UserFilter(Email: email));
        if (user is null)
        {
            throw new KeyNotFoundException($"User not found with email {email}");
        }
        var auth = (Auth)user!;
        return auth;
    }

    public record Filter(
        string? UserName = null,
        List<UserRole>? Roles = null,
        DateTimeRange? DateRange = null,
        List<long>? UserIds = null,
        List<UserRole>? UserRoles = null,
        List<long>? QuizIds = null,
        List<long>? ExamIds = null
    );
}
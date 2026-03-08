using Chik.Exams.Logins.Repositories;

namespace Chik.Exams;

public interface ILoginService
{
    public ILoginRepository Repository { get; }

    /// <summary>
    /// Authenticates a user with username and password. Returns the user if successful.
    /// </summary>
    Task<User> Authenticate(string username, string password, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Records a login event for a user.
    /// </summary>
    Task Create(Auth auth, Login.Create login);

    /// <summary>
    /// Searches for login events.
    /// </summary>
    Task<Paginated<Login>> Search(Auth auth, Login.Filter? filter = null, PaginationOptions? pagination = null);
}
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
    /// Generates access and refresh tokens for a user.
    /// </summary>
    (string accessToken, string refreshToken) GenerateTokens(User user);

    /// <summary>
    /// Refreshes tokens using a valid refresh token. Returns new access and refresh tokens.
    /// </summary>
    Task<(string accessToken, string refreshToken)> RefreshTokens(string refreshToken);

    /// <summary>
    /// Verifies a token and returns the claims if valid.
    /// </summary>
    Task<IEnumerable<System.Security.Claims.Claim>> VerifyToken(string token);

    /// <summary>
    /// Records a login event for a user.
    /// </summary>
    Task Create(Auth auth, Login.Create login);

    /// <summary>
    /// Searches for login events.
    /// </summary>
    Task<Paginated<Login>> Search(Auth auth, Login.Filter? filter = null, PaginationOptions? pagination = null);
}
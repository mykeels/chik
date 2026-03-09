using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chik.Exams.Api;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ILoginService _loginService;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ILoginService loginService,
        IUserService userService,
        ILogger<AuthController> logger)
    {
        _loginService = loginService;
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user with username and password.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var ipAddress = Request.GetClientIpAddress();
        var userAgent = Request.Headers.UserAgent.FirstOrDefault();

        var user = await _loginService.Authenticate(
            request.Username,
            request.Password,
            ipAddress,
            userAgent);

        var (accessToken, refreshToken) = _loginService.GenerateTokens(user);
        AuthenticationExtensions.SaveCookies(accessToken, refreshToken, user.Id.ToString());

        _logger.LogInformation("User {Username} logged in successfully", user.Username);

        return Ok(new LoginResponse(
            user.Id,
            user.Username,
            user.Roles,
            "Login successful"));
    }

    /// <summary>
    /// Refreshes the access token using the refresh token from cookies.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult> RefreshToken()
    {
        var refreshToken = Request.GetRefreshToken();
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { Message = "No refresh token provided" });
        }

        try
        {
            var (newAccessToken, newRefreshToken) = await _loginService.RefreshTokens(refreshToken);
            
            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(newAccessToken);
            var userId = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            
            AuthenticationExtensions.SaveCookies(newAccessToken, newRefreshToken, userId);
            
            return Ok(new { Message = "Token refreshed successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { Message = "Invalid or expired refresh token" });
        }
    }

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await AuthenticationExtensions.Logout();
        return Ok(new { Message = "Logged out successfully" });
    }

    /// <summary>
    /// Changes the password for the current user.
    /// </summary>
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        [FromServices] Auth auth)
    {
        await _userService.ChangePassword(
            auth,
            auth.Id,
            request.CurrentPassword,
            request.NewPassword);

        return Ok(new { Message = "Password changed successfully" });
    }

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    [HttpGet("me")]
    public ActionResult<Auth> GetCurrentUser([FromServices] Auth auth)
    {
        return Ok(auth);
    }
}

/// <summary>
/// Request model for login.
/// </summary>
public record LoginRequest(
    [Required] string Username,
    [Required] string Password
);

/// <summary>
/// Response model for successful login.
/// </summary>
public record LoginResponse(
    long Id,
    string Username,
    List<UserRole> Roles,
    string Message
);

/// <summary>
/// Request model for changing password.
/// </summary>
public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required] string NewPassword
);

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
        try
        {
            var ipAddress = Request.GetClientIpAddress();
            var userAgent = Request.Headers.UserAgent.FirstOrDefault();
            
            var user = await _loginService.Authenticate(
                request.Username, 
                request.Password, 
                ipAddress, 
                userAgent);
            
            _logger.LogInformation("User {Username} logged in successfully", user.Username);
            
            return Ok(new LoginResponse(
                user.Id,
                user.Username,
                user.Roles,
                "Login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
            return Unauthorized(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        try
        {
            await AuthenticationExtensions.Logout();
            return Ok(new { Message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { Message = "Error during logout" });
        }
    }

    /// <summary>
    /// Changes the password for the current user.
    /// </summary>
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        [FromServices] Auth auth)
    {
        try
        {
            await _userService.ChangePassword(
                auth, 
                auth.Id, 
                request.CurrentPassword, 
                request.NewPassword);
            
            return Ok(new { Message = "Password changed successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
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

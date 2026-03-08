using Microsoft.AspNetCore.Mvc;

namespace Chik.Exams.Api;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new user. Admin can create any role, Teacher can create Student only.
    /// </summary>
    [HttpPost]
    [AdminOrTeacher]
    public async Task<ActionResult<User>> Create(
        [FromBody] CreateUserRequest request,
        [FromServices] Auth auth)
    {
        var user = await _userService.Create(auth, new User.Create(
            request.Username,
            request.Password,
            request.Roles));

        _logger.LogInformation("User {Username} created by {Creator}", user.Username, auth.Username);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    /// <summary>
    /// Gets a user by ID. Admin can get any user, others can only get themselves.
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<User>> Get(long id, [FromServices] Auth auth)
    {
        var user = await _userService.Get(auth, id);
        if (user is null)
        {
            return NotFound(new { Message = "User not found" });
        }
        return Ok(user);
    }

    /// <summary>
    /// Updates a user. Admin can update any user, others can only update themselves (except roles).
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<User>> Update(
        long id,
        [FromBody] UpdateUserRequest request,
        [FromServices] Auth auth)
    {
        var user = await _userService.Update(auth, new User.Update(
            id,
            request.Username,
            request.Password,
            request.Roles));

        return Ok(user);
    }

    /// <summary>
    /// Changes password for a user. Users can only change their own password.
    /// </summary>
    [HttpPost("{id:long}/change-password")]
    public async Task<ActionResult> ChangePassword(
        long id,
        [FromBody] ChangePasswordRequest request,
        [FromServices] Auth auth)
    {
        await _userService.ChangePassword(auth, id, request.CurrentPassword, request.NewPassword);
        return Ok(new { Message = "Password changed successfully" });
    }

    /// <summary>
    /// Deletes a user. Admin only.
    /// </summary>
    [HttpDelete("{id:long}")]
    [AdminOnly]
    public async Task<ActionResult> Delete(long id, [FromServices] Auth auth)
    {
        await _userService.Delete(auth, id);
        return NoContent();
    }

    /// <summary>
    /// Searches for users. Admin can search all, Teacher can search students, Student can only see themselves.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Paginated<User>>> Search(
        [FromServices] Auth auth,
        [FromQuery] User.Filter? filter,
        [FromQuery] PaginationOptions? pagination)
    {
        var result = await _userService.Search(auth, filter, pagination);

        return Ok(result);
    }
}

/// <summary>
/// Request model for creating a user.
/// </summary>
public record CreateUserRequest(
    [Required] string Username,
    [Required] string Password,
    [Required] List<UserRole> Roles
);

/// <summary>
/// Request model for updating a user.
/// </summary>
public record UpdateUserRequest(
    string? Username = null,
    string? Password = null,
    List<UserRole>? Roles = null
);

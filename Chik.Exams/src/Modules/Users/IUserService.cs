using Chik.Exams.Data;

namespace Chik.Exams;

public interface IUserService
{
    public IUserRepository Repository { get; }

    /// <summary>
    /// Creates a new user. Admin can create any role, Teacher can create Student only.
    /// </summary>
    Task<User> Create(Auth auth, User.Create user);

    /// <summary>
    /// Gets a user by ID. Admin can get any user, others can only get themselves.
    /// </summary>
    Task<User?> Get(Auth auth, long id);

    /// <summary>
    /// Updates a user. Admin can update any user, others can only update themselves (except roles).
    /// </summary>
    Task<User> Update(Auth auth, User.Update user);

    /// <summary>
    /// Changes the password for a user. Users can only change their own password.
    /// </summary>
    Task ChangePassword(Auth auth, long userId, string currentPassword, string newPassword);

    /// <summary>
    /// Deletes a user. Admin only.
    /// </summary>
    Task Delete(Auth auth, long id);

    /// <summary>
    /// Searches for users. Admin can search all, Teacher can search students, Student can only see themselves.
    /// </summary>
    Task<Paginated<User>> Search(Auth auth, User.Filter? filter = null, PaginationOptions? pagination = null);
}
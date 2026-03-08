using Chik.Exams.Data;

namespace Chik.Exams;

internal class UserService(
    IUserRepository repository,
    IAuditLogService auditLogService,
    ILogger<UserService> logger
) : IUserService
{
    public IUserRepository Repository => repository;

    public async Task<User> Create(Auth auth, User.Create user)
    {
        logger.LogInformation($"{nameof(UserService)}.{nameof(Create)} ({auth.Id}, {user})");
        
        // Authorization: Admin can create any role, Teacher can only create Student
        if (!auth.IsAdmin())
        {
            if (!auth.IsTeacher())
            {
                throw new UnauthorizedAccessException("Only Admin or Teacher can create users");
            }
            // Teacher can only create students
            if (user.Roles.Any(r => r != UserRole.Student))
            {
                throw new UnauthorizedAccessException("Teachers can only create Student users");
            }
        }

        var userDbo = await repository.Create(user);
        await auditLogService.Create(
            auth, 
            new AuditLog.Create<User.Create>(
                $"{nameof(UserService)}.{nameof(Create)}", 
                userDbo.Id, 
                user
            )
        );
        return userDbo!.ToModel();
    }

    public async Task<User?> Get(Auth auth, long id)
    {
        logger.LogInformation($"{nameof(UserService)}.{nameof(Get)} ({auth.Id}, {id})");
        
        // Authorization: Admin can get any user, others can only get themselves
        if (!auth.IsAdmin() && auth.Id != id)
        {
            throw new UnauthorizedAccessException("You can only view your own profile");
        }

        var userDbo = await repository.Get(id);
        return userDbo!.ToModel();
    }

    public async Task<User> Update(Auth auth, User.Update user)
    {
        logger.LogInformation($"{nameof(UserService)}.{nameof(Update)} ({auth.Id}, {user})");
        
        // Authorization: Admin can update any user, others can only update themselves
        if (!auth.IsAdmin() && auth.Id != user.Id)
        {
            throw new UnauthorizedAccessException("You can only update your own profile");
        }

        // Non-admins cannot change roles
        if (!auth.IsAdmin() && user.Roles is not null)
        {
            throw new UnauthorizedAccessException("Only Admin can change user roles");
        }

        var userDbo = await repository.Update(user.Id, user);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<User.Update>(
                $"{nameof(UserService)}.{nameof(Update)}",
                user.Id,
                user
            )
        );
        return userDbo!.ToModel();
    }

    public async Task ChangePassword(Auth auth, long userId, string currentPassword, string newPassword)
    {
        logger.LogInformation($"{nameof(UserService)}.{nameof(ChangePassword)} ({auth.Id}, {userId})");
        
        // Authorization: Users can only change their own password
        if (auth.Id != userId)
        {
            throw new UnauthorizedAccessException("You can only change your own password");
        }

        var userDbo = await repository.Get(userId);
        if (userDbo is null)
        {
            throw new KeyNotFoundException($"User with id '{userId}' not found");
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, userDbo.Password))
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        await repository.Update(userId, new User.Update(userId, Password: newPassword));
        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(UserService)}.{nameof(ChangePassword)}",
                userId,
                new { } // Don't log password changes
            )
        );
    }

    public async Task Delete(Auth auth, long id)
    {
        logger.LogInformation($"{nameof(UserService)}.{nameof(Delete)} ({auth.Id}, {id})");
        
        // Authorization: Admin only
        if (!auth.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only Admin can delete users");
        }

        await repository.Delete(id);
        await auditLogService.Create(
            auth,
            new AuditLog.Create<object>(
                $"{nameof(UserService)}.{nameof(Delete)}",
                id,
                new { DeletedUserId = id }
            )
        );
    }

    public async Task<Paginated<User>> Search(Auth auth, User.Filter? filter = null, PaginationOptions? pagination = null)
    {
        logger.LogInformation($"{nameof(UserService)}.{nameof(Search)} ({auth.Id}, {filter})");
        
        filter ??= new User.Filter();
        pagination ??= new PaginationOptions();

        // Authorization: Admin can search all, Teacher can search students, Student can only see themselves
        if (!auth.IsAdmin())
        {
            if (auth.IsTeacher())
            {
                // Teachers can only see students
                filter = filter with { Roles = new List<UserRole> { UserRole.Student }.ToInt32() };
            }
            else
            {
                // Students can only see themselves
                filter = filter with { UserIds = [auth.Id] };
            }
        }

        var paginated = await repository.Search(filter, pagination);
        return new Paginated<User>(
            paginated.Items.Select(dbo => (User)dbo!.ToModel()).ToList(),
            paginated.TotalCount,
            pagination,
            async options => await Search(auth, filter, options)
        );
    }

    public class Cache
    {
        public class Keys
        {
            public static string User(Guid userId) => $"user:id={userId}";
        }

        public class Tags
        {
            public static string User(Guid id) => $"user:id={id}";
        }
    }
}
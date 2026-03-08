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
        var userDbo = await repository.Create(user);
        await auditLogService.Create(
            auth, 
            new AuditLog.Create<User.Create>(
                $"{nameof(UserService)}.{nameof(Create)}", 
                userDbo.Id, 
                user
            )
        );
        return new User(userDbo.Id, userDbo.Username, UserRoleExtensions.FromInt32(userDbo.Roles), userDbo.CreatedAt, userDbo.UpdatedAt);
    }
}
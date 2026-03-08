using Microsoft.AspNetCore.Authorization;

namespace Chik.Exams.Api;

/// <summary>
/// Authorization attribute that requires the user to have at least one of the specified roles.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireRolesAttribute : AuthorizeAttribute, IAuthorizationRequirement
{
    public UserRole[] Roles { get; }

    public RequireRolesAttribute(params UserRole[] roles)
    {
        Roles = roles;
    }
}

/// <summary>
/// Requires Admin role.
/// </summary>
public class AdminOnlyAttribute : RequireRolesAttribute
{
    public AdminOnlyAttribute() : base(UserRole.Admin) { }
}

/// <summary>
/// Requires Admin or Teacher role.
/// </summary>
public class AdminOrTeacherAttribute : RequireRolesAttribute
{
    public AdminOrTeacherAttribute() : base(UserRole.Admin, UserRole.Teacher) { }
}

/// <summary>
/// Requires any authenticated user (Admin, Teacher, or Student).
/// </summary>
public class AnyRoleAttribute : RequireRolesAttribute
{
    public AnyRoleAttribute() : base(UserRole.Admin, UserRole.Teacher, UserRole.Student) { }
}

/// <summary>
/// Requires Student role (or Admin/Teacher who can act on behalf of students).
/// </summary>
public class StudentAccessAttribute : RequireRolesAttribute
{
    public StudentAccessAttribute() : base(UserRole.Admin, UserRole.Teacher, UserRole.Student) { }
}

/// <summary>
/// Handler for role-based authorization requirements.
/// </summary>
public class RoleRequirementHandler : AuthorizationHandler<RequireRolesAttribute>
{
    private readonly IServiceProvider _serviceProvider;

    public RoleRequirementHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequireRolesAttribute requirement)
    {
        using var scope = _serviceProvider.CreateScope();
        
        try
        {
            var auth = scope.ServiceProvider.GetRequiredService<Auth>();
            
            // Check if user has at least one of the required roles
            if (requirement.Roles.Any(role => auth.HasRole(role)))
            {
                context.Succeed(requirement);
            }
        }
        catch
        {
            // If we can't get the auth, the requirement fails
        }
        
        return Task.CompletedTask;
    }
}

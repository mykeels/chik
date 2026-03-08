using Microsoft.AspNetCore.Authorization;

namespace Chik.Exams.Api;

public static class AuthorizationExtensions
{
    /// <summary>
    /// Adds role-based authorization policies and handlers.
    /// </summary>
    public static IServiceCollection AddRoleAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, RoleRequirementHandler>();
        
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy =>
                policy.Requirements.Add(new RequireRolesAttribute(UserRole.Admin)))
            .AddPolicy("AdminOrTeacher", policy =>
                policy.Requirements.Add(new RequireRolesAttribute(UserRole.Admin, UserRole.Teacher)))
            .AddPolicy("AnyRole", policy =>
                policy.Requirements.Add(new RequireRolesAttribute(UserRole.Admin, UserRole.Teacher, UserRole.Student)));
        
        return services;
    }
}

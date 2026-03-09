using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using ZiggyCreatures.Caching.Fusion;

namespace Chik.Exams.Api;

public static class AuthenticationExtensions
{
    private static class CookieNames
    {
        public const string AccessToken = "app.at";
        public const string RefreshToken = "app.rt";
        public const string UserId = "app.uid";
    }

    public static string GetAccessToken(this HttpRequest request)
    {
        if (request.Cookies.TryGetValue(CookieNames.AccessToken, out string? token))
        {
            return token;
        }
        return string.Empty;
    }

    public static string GetRefreshToken(this HttpRequest request)
    {
        if (request.Cookies.TryGetValue(CookieNames.RefreshToken, out string? token))
        {
            return token;
        }
        return string.Empty;
    }

    public static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        IdentityModelEventSource.ShowPII = true;
        IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        services.AddHttpContextAccessor();
        
        var jwtConfig = configuration.GetSection("Jwt").Get<JwtConfig>() 
            ?? throw new Exception("Jwt configuration is not set");
        
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = jwtConfig.SigningCredentials.Key,
                ValidateIssuer = !string.IsNullOrEmpty(jwtConfig.Issuer),
                ValidIssuer = jwtConfig.Issuer,
                ValidateAudience = !string.IsNullOrEmpty(jwtConfig.Audience),
                ValidAudience = jwtConfig.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };
            options.Events = new()
            {
                OnMessageReceived = async context =>
                {
                    var logger = Provider.GetRequiredService<ILogger<Program>>();
                    var loginService = Provider.GetRequiredService<ILoginService>();

                    // First try to get token from Authorization header (standard JWT Bearer)
                    if (string.IsNullOrEmpty(context.Token))
                    {
                        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                        if (authHeader?.StartsWith("Bearer ") == true)
                        {
                            context.Token = authHeader.Substring("Bearer ".Length).Trim();
                        }
                    }

                    // If no token in header, try to get from cookies
                    if (string.IsNullOrEmpty(context.Token))
                    {
                        context.Token = context.Request.GetAccessToken();
                    }

                    var refreshToken = context.Request.GetRefreshToken();
                    logger.Info("Token received", new { HasToken = !string.IsNullOrEmpty(context.Token), HasRefreshToken = !string.IsNullOrEmpty(refreshToken) });

                    if (!string.IsNullOrEmpty(context.Token))
                    {
                        var token = new JwtSecurityTokenHandler().ReadJwtToken(context.Token);
                        logger.Info("Token ValidTo", new { token.ValidTo });
                        if (token.ValidTo < DateTime.UtcNow && !string.IsNullOrEmpty(refreshToken))
                        {
                            try
                            {
                                context.Token = await RefreshAccessToken(refreshToken, loginService);
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, new { Message = "Failed to refresh accessToken" });
                                context.Token = string.Empty;
                            }
                        }
                    }
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogInformation("Token validated: {Principal}", context.Principal?.Identity?.Name);
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogError("JWT Authentication failed: {Exception}", context.Exception);
                    return Task.CompletedTask;
                }
            };
        })
        .AddCookie(options =>
        {
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });
        services.AddScoped((sp) =>
        {
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            var auth = GetAuth(httpContextAccessor.HttpContext?.User ?? throw new UnauthorizedAccessException()).Result;
            return auth;
        });
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(
                new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(
                        [
                            JwtBearerDefaults.AuthenticationScheme,
                            CookieAuthenticationDefaults.AuthenticationScheme
                        ]
                    )
                    .Build()
            );
        return services;
    }

    private static async Task<string> RefreshAccessToken(string refreshToken, ILoginService? loginService = null, ILogger? logger = null)
    {
        loginService ??= Provider.GetRequiredService<ILoginService>();
        logger ??= Provider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var (newAccessToken, newRefreshToken) = await loginService.RefreshTokens(refreshToken);
            
            var token = new JwtSecurityTokenHandler().ReadJwtToken(newAccessToken);
            var userId = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            
            logger.Info("Access token refreshed successfully");
            SaveCookies(newAccessToken, newRefreshToken, userId);
            return newAccessToken;
        }
        catch (Exception ex)
        {
            logger.Warn("Failed to refresh access token", new { Error = ex.Message });
            await Logout();
            throw new UnauthorizedAccessException("Failed to refresh access token", ex);
        }
    }

    public static void SaveCookies(
        string accessToken, 
        string? refreshToken, 
        string? userId, 
        IHttpContextAccessor? httpContextAccessor = null,
        RemoteEnvironment? remoteEnvironment = null
    )
    {
        httpContextAccessor ??= Provider.GetRequiredService<IHttpContextAccessor>();
        remoteEnvironment ??= Provider.GetRequiredService<RemoteEnvironment>();
        if (accessToken is not null)
        {
            var expiryDate = new JwtSecurityTokenHandler().ReadJwtToken(accessToken).ValidTo.AddDays(1);
            httpContextAccessor.HttpContext?.Response.Cookies.Append(
                CookieNames.AccessToken,
                accessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = expiryDate,
                    SameSite = SameSiteMode.None,
                    Domain = remoteEnvironment.GetCookieDomain()
                }
            );
        }
        if (refreshToken is not null)
        {
            httpContextAccessor.HttpContext?.Response.Cookies.Append(
                CookieNames.RefreshToken,
                refreshToken,
                new CookieOptions 
                { 
                    HttpOnly = true, 
                    Secure = true,
                    Expires = DateTime.UtcNow.AddDays(30),
                    Domain = remoteEnvironment.GetCookieDomain()
                }
            );
        }
        if (userId is not null)
        {
            httpContextAccessor.HttpContext?.Response.Cookies.Append(
                CookieNames.UserId,
                userId,
                new CookieOptions 
                { 
                    HttpOnly = true, 
                    Secure = true,
                    Expires = DateTime.UtcNow.AddDays(60),
                    Domain = remoteEnvironment.GetCookieDomain()
                }
            );
        }
    }

    private static async Task<Auth> GetAuth(
        this ClaimsPrincipal principal,
        IFusionCache? fusionCache = null,
        IUserService? userService = null,
        IHttpContextAccessor? httpContextAccessor = null
    )
    {
        fusionCache ??= Provider.GetRequiredService<IFusionCache>();
        userService ??= Provider.GetRequiredService<IUserService>();
        httpContextAccessor ??= Provider.GetRequiredService<IHttpContextAccessor>();
        var logger = Provider.GetRequiredService<ILogger<Auth>>();
        
        // Get user ID from the "sub" claim (set by our local JWT)
        var subClaim = principal.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(subClaim) || !long.TryParse(subClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user ID in token");
        }
        
        // track id as a cache tag
        string[] cacheTags = [ UserService.Cache.Tags.User(userId) ];
        var auth = await fusionCache.GetOrSetAsync(
            UserService.Cache.Keys.User(userId),
            async (_) =>
            {
                logger.LogInformation("Retrieving user: {UserId}", userId);
                
                var user = await userService.Get(User.Admin, userId);
                if (user is null)
                {
                    httpContextAccessor.ClearAuthCookies();
                    throw new UnauthorizedAccessException("User not found");
                }
                
                logger.LogInformation("User retrieved: {Username}", user.Username);
                return (Auth)user;
            },
            TimeSpan.FromHours(1),
            tags: cacheTags
        );
        return auth;
    }

    public static async Task Logout(
        IHttpContextAccessor? httpContextAccessor = null, 
        RemoteEnvironment? remoteEnvironment = null
    )
    {
        httpContextAccessor ??= Provider.GetRequiredService<IHttpContextAccessor>();
        remoteEnvironment ??= Provider.GetRequiredService<RemoteEnvironment>();
        
        ClearAuthCookies(httpContextAccessor, remoteEnvironment);
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false)
        {
            await httpContextAccessor.HttpContext.SignOutAsync();
        }
    }

    public static void ClearAuthCookies(
        this IHttpContextAccessor httpContextAccessor, 
        RemoteEnvironment? remoteEnvironment = null
    )
    {
        httpContextAccessor ??= Provider.GetRequiredService<IHttpContextAccessor>();
        remoteEnvironment ??= Provider.GetRequiredService<RemoteEnvironment>();
        httpContextAccessor.HttpContext?.Response.Cookies.Delete(
            CookieNames.AccessToken,
            new CookieOptions
            {
                Domain = remoteEnvironment.GetCookieDomain()
            }
        );
        httpContextAccessor.HttpContext?.Response.Cookies.Delete(
            CookieNames.RefreshToken,
            new CookieOptions
            {
                Domain = remoteEnvironment.GetCookieDomain()
            }
        );
        httpContextAccessor.HttpContext?.Response.Cookies.Delete(
            CookieNames.UserId,
            new CookieOptions
            {
                Domain = remoteEnvironment.GetCookieDomain()
            }
        );
    }

    public static async Task Logout(
        this Auth auth, 
        IHttpContextAccessor? httpContextAccessor = null, 
        RemoteEnvironment? remoteEnvironment = null
    )
    {
        await Logout(httpContextAccessor, remoteEnvironment);
    }

}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using io.fusionauth;
using io.fusionauth.jwt.domain;
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

    public static Guid TenantId => Guid.Parse(Provider.GetRequiredService<IConfiguration>()["OpenIdConnect:TenantId"] ?? throw new Exception("OpenIdConnect:TenantId is not set"));
    public static string ClientId => Provider.GetRequiredService<IConfiguration>()["OpenIdConnect:ClientId"] ?? throw new Exception("OpenIdConnect:ClientId is not set");
    public static string ClientSecret => Provider.GetRequiredService<IConfiguration>()["OpenIdConnect:ClientSecret"] ?? throw new Exception("OpenIdConnect:ClientSecret is not set");
    public static string[] Scope => Provider.GetRequiredService<IConfiguration>().GetSection("OpenIdConnect:Scope").Get<string[]>() ?? throw new Exception("OpenIdConnect:Scope is not set");

    public static string GetAccessToken(this HttpRequest request)
    {
        string[] cookieNames = new[] { CookieNames.AccessToken, "access_token", "fusionauth.at" };
        foreach (string cookieName in cookieNames)
        {
            if (request.Cookies.TryGetValue(cookieName, out string? token))
            {
                return token;
            }
        }
        return string.Empty;
    }

    public static string GetRefreshToken(this HttpRequest request)
    {
        string[] cookieNames = new[] { CookieNames.RefreshToken, "refresh_token", "fusionauth.rt" };
        foreach (string cookieName in cookieNames)
        {
            if (request.Cookies.TryGetValue(cookieName, out string? token))
            {
                return token;
            }
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
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            configuration.GetSection("Authentication").Bind(options);
            options.Events = new()
            {
                OnMessageReceived = async context =>
                {
                    var logger = Provider.GetRequiredService<ILogger<Program>>();
                    var httpContextAccessor = Provider.GetRequiredService<IHttpContextAccessor>();
                    var fusionAuthClient = Provider.GetRequiredService<IFusionAuthAsyncClient>();

                    // First try to get token from Authorization header (standard JWT Bearer)
                    if (string.IsNullOrEmpty(context.Token))
                    {
                        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                        if (authHeader?.StartsWith("Bearer ") == true)
                        {
                            context.Token = authHeader.Substring("Bearer ".Length).Trim();
                        }
                    }

                    // If no token in header, try to get from cookies (for backward compatibility)
                    if (string.IsNullOrEmpty(context.Token))
                    {
                        context.Token = context.Request.GetAccessToken();
                    }

                    var refreshToken = context.Request.GetRefreshToken();
                    logger.Info("Token received", new { context.Token, refreshToken });

                    if (!string.IsNullOrEmpty(context.Token))
                    {
                        var token = new JwtSecurityTokenHandler().ReadJwtToken(context.Token);
                        logger.Info("Token ValidTo", new { token.ValidTo });
                        if (token.ValidTo < DateTime.UtcNow)
                        {
                            try
                            {
                                context.Token = await RefreshAccessToken(refreshToken, token.Subject, fusionAuthClient);
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, new { Message = "Failed to refresh accessToken", refreshToken, token.Subject });
                                context.Token = string.Empty;
                            }
                        }
                    }
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogInformation("Token validated: {Principal}", context.Principal);
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
        .AddOpenIdConnect(options =>
        {
            var section = configuration.GetSection("OpenIdConnect");
            section.Bind(options);

            // Configure for FusionAuth
            string tenantId = section.GetValue<string>("TenantId") ?? "54b9787c-7956-418b-a173-447d9e97d1bd";
            options.MetadataAddress = $"{options.Authority}/{tenantId}/.well-known/openid-configuration";
            options.RequireHttpsMetadata = false;
        })
        .AddCookie(options =>
        {
            options.Events.OnSigningIn = context =>
            {
                var logger = Provider.GetRequiredService<ILogger<Program>>();
                var httpContextAccessor = Provider.GetRequiredService<IHttpContextAccessor>();
                var userId = context.Principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                SaveCookies(
                    context.Properties.GetTokenValue("access_token") ?? throw new UnauthorizedAccessException("No access token found in AuthenticationProperties"),
                    context.Properties.GetTokenValue("refresh_token") ?? throw new UnauthorizedAccessException("No refresh token found in AuthenticationProperties"),
                    userId ?? throw new UnauthorizedAccessException("No user id found in AuthenticationProperties"),
                    httpContextAccessor
                );
                context.Principal?.AddIdentity(
                    new ClaimsIdentity()
                );
                return Task.CompletedTask;
            };
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

    private static async Task<string> RefreshAccessToken(string refreshToken, string userCode, IFusionAuthAsyncClient? fusionAuthClient = null, ILogger? logger = null)
    {
        fusionAuthClient ??= Provider.GetRequiredService<IFusionAuthAsyncClient>();
        logger ??= Provider.GetRequiredService<ILogger<Program>>();
        var res = await fusionAuthClient.ExchangeRefreshTokenForAccessTokenAsync(
            refresh_token: refreshToken,
            client_id: ClientId,
            client_secret: ClientSecret,
            scope: string.Join(" ", Scope),
            user_code: userCode
        );
        if (res.errorResponse is not null && res.successResponse is null)
        {
            logger.Warn("Failed to exchange refresh token for access token", new {
                res.errorResponse,
                userCode,
                refreshToken
            });
            await Logout();
            throw new FusionAuthClientException(
                "Failed to exchange refresh token for access token",
                res.errorResponse
            );
        }
        logger.Info("Access token refreshed successfully", res.successResponse);
        SaveCookies(
            res.successResponse?.access_token ?? throw new Exception("No access token returned"),
            res.successResponse?.refresh_token,
            res.successResponse?.userId?.ToString()
        );
        return res.successResponse?.access_token ?? throw new Exception("No access token returned");
    }

    private static void SaveCookies(
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
        IFusionAuthAsyncClient? fusionAuthClient = null,
        IFusionCache? fusionCache = null,
        IUserService? userService = null,
        ILoginService? loginService = null,
        IIpAddressLocationService? ipAddressLocationService = null,
        IHttpContextAccessor? httpContextAccessor = null,
        TimeProvider? timeProvider = null
    )
    {
        fusionAuthClient ??= Provider.GetRequiredService<IFusionAuthAsyncClient>();
        fusionCache ??= Provider.GetRequiredService<IFusionCache>();
        userService ??= Provider.GetRequiredService<IUserService>();
        loginService ??= Provider.GetRequiredService<ILoginService>();
        ipAddressLocationService ??= Provider.GetRequiredService<IIpAddressLocationService>();
        httpContextAccessor ??= Provider.GetRequiredService<IHttpContextAccessor>();
        timeProvider ??= Provider.GetRequiredService<TimeProvider>();
        var logger = Provider.GetRequiredService<ILogger<Auth>>();
        Guid fusionAuthId = Guid.Parse(principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        // track id as a cache tag
        string[] cacheTags = [ UserService.Cache.Tags.User(fusionAuthId) ];
        var auth = await fusionCache.GetOrSetAsync(
            UserService.Cache.Keys.User(fusionAuthId),
            async (_) =>
            {
                long authenticatedAtOffset = long.Parse(principal.Claims.FirstOrDefault(c => c.Type == "auth_time")?.Value ?? throw new UnauthorizedAccessException());
                DateTime authenticatedAt = DateTimeOffset.FromUnixTimeSeconds(authenticatedAtOffset).UtcDateTime;
                string email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? throw new UnauthorizedAccessException();

                logger.LogInformation($"Retrieving user: {fusionAuthId}");
                var res = await fusionAuthClient.RetrieveUserAsync(fusionAuthId);
                if (res.errorResponse is not null)
                {
                    string error = string.Join(
                        "\n",
                        new List<string>()
                        .Concat(res.errorResponse.fieldErrors.Select(e => $"{e.Key}: {e.Value}"))
                        .Concat(res.errorResponse.generalErrors.Select(e => $"{e.code}: {e.message}, {e.data}"))
                    );
                    throw new UnauthorizedAccessException(
                        $"Failed to retrieve user: {error}"
                    );
                }
                logger.LogInformation("User retrieved: {User}", res.successResponse?.user?.email);
                if (res.successResponse?.user?.email is null)
                {
                    httpContextAccessor.ClearAuthCookies();
                    throw new UnauthorizedAccessException("User not found");
                }
                string name = res.successResponse.user.fullName;
                DateOnly dateOfBirth = DateOnly.Parse(res.successResponse.user.birthDate);
                bool isVerified = res.successResponse.user.verified ?? false;
                string username = email.Split('@')[0];
                
                // Try to get or create user in local database
                var existingUsers = await userService.Search(User.Admin, new User.Filter(Username: username), new PaginationOptions(1, 1));
                Auth auth;
                if (existingUsers.TotalCount > 0)
                {
                    auth = existingUsers.Items.First();
                }
                else
                {
                    // Create new user in local database
                    auth = await userService.Create(User.Admin, new User.Create(username, "", [UserRole.Teacher]));
                }
                
                await auth.TrackLogin();
                return auth;
            },
            TimeSpan.FromHours(1),
            tags: cacheTags
        );
        return auth;
    }

    public static async Task Logout(
        IHttpContextAccessor? httpContextAccessor = null, 
        IFusionAuthAsyncClient? fusionAuthClient = null,
        RemoteEnvironment? remoteEnvironment = null
    )
    {
        httpContextAccessor ??= Provider.GetRequiredService<IHttpContextAccessor>();
        fusionAuthClient ??= Provider.GetRequiredService<IFusionAuthAsyncClient>();
        remoteEnvironment ??= Provider.GetRequiredService<RemoteEnvironment>();
        string? refreshToken = httpContextAccessor.HttpContext?.Request.GetRefreshToken();
        if (refreshToken is not null)
        {
            await fusionAuthClient.LogoutAsync(true, refreshToken);
        }
        ClearAuthCookies(httpContextAccessor, remoteEnvironment);
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false)
        {
            await httpContextAccessor.HttpContext?.SignOutAsync();
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
        IFusionAuthAsyncClient? fusionAuthClient = null,
        RemoteEnvironment? remoteEnvironment = null
    )
    {
        await Logout(httpContextAccessor, fusionAuthClient, remoteEnvironment);
    }

    private static async Task TrackLogin(this Auth auth, IHttpContextAccessor? httpContextAccessor = null, IIpAddressLocationService? ipAddressLocationService = null, ILoginService? loginService = null, ILogger? logger = null)
    {
        httpContextAccessor ??= Provider.GetRequiredService<IHttpContextAccessor>();
        ipAddressLocationService ??= Provider.GetRequiredService<IIpAddressLocationService>();
        loginService ??= Provider.GetRequiredService<ILoginService>();
        logger ??= Provider.GetRequiredService<ILogger<Auth>>();
        string? ipAddress = httpContextAccessor.HttpContext?.Request.GetClientIpAddress();
        string? ipAddressCountry = httpContextAccessor.HttpContext?.Request.GetClientIpAddressCountry();
        if (ipAddress is not null && ipAddressCountry is not null)
        {
            logger.LogInformation(
                $"{nameof(AuthenticationExtensions)}.{nameof(GetAuth)} ({auth.Id}, {ipAddress}, {ipAddressCountry})"
            );
            var ipAddressLocation = await ipAddressLocationService.Create(new IpAddressLocation.Create(
                ipAddress,
                ipAddressCountry
            ));
            await loginService.Create(auth, new Login.Create(
                auth.Id,
                ipAddressLocation.Id
            ));
        }
    }

    private record JwkList(List<JsonWebKey> Keys);
}
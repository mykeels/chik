using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Chik.Exams;

public class JwtService(
        JwtConfig config,
        TimeProvider timeProvider,
        ILogger<JwtService> logger,
        IHttpClientFactory? httpClientFactory = null
    ) : IJwtService
    {
        private readonly JwtSecurityTokenHandler _handler = new();

        private readonly Dictionary<
            string,
            ConfigurationManager<OpenIdConnectConfiguration>
        > _configurationManagers = new();

        public JwtConfig Config { get; } = config;

        public string GenerateToken(
            Dictionary<string, string> claims,
            DateTime? absoluteExpireTimeUtc = null
        )
        {
            absoluteExpireTimeUtc ??= timeProvider.GetUtcNow().Add(Config.TokenExpiration).DateTime;
            var canUseJwks = CanUseJwks(Config.Issuer);
            if (canUseJwks)
            {
                throw new NotImplementedException(
                    $"With this configuration, JWT signing can only be done by the Identity Server Issuer: {Config.Issuer}"
                );
            }

            return WriteToken(
                claims.Select(x => new Claim(x.Key, x.Value)),
                absoluteExpireTimeUtc.Value
            );
        }

        public string GenerateToken(
            IEnumerable<Claim> claims,
            DateTime? absoluteExpireTimeUtc = null
        )
        {
            absoluteExpireTimeUtc ??= timeProvider.GetUtcNow().Add(Config.TokenExpiration).DateTime;
            var canUseJwks = CanUseJwks(Config.Issuer);
            if (canUseJwks)
            {
                throw new NotImplementedException(
                    $"With this configuration, JWT signing can only be done by the Identity Server Issuer: {Config.Issuer}"
                );
            }

            return WriteToken(claims, absoluteExpireTimeUtc.Value);
        }

        private string WriteToken(IEnumerable<Claim> claims, DateTime absoluteExpireTime)
        {
            return _handler.WriteToken(
                new JwtSecurityToken(
                    Config.Issuer,
                    Config.Audience,
                    claims,
                    expires: absoluteExpireTime,
                    signingCredentials: Config.SigningCredentials
                )
            );
        }

        public async Task<IEnumerable<Claim>> VerifyToken(string token)
        {
            try
            {
                var canUseJwks = CanUseJwks(Config.Issuer);
                if (canUseJwks)
                {
                    return await VerifyTokenWithJwks(token, Config.Issuer);
                }

                var claims = _handler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = Config.SigningCredentials.Key,
                        ValidateIssuer = !string.IsNullOrEmpty(Config.Issuer),
                        ValidIssuer = Config.Issuer,
                        ValidateAudience = !string.IsNullOrEmpty(Config.Audience),
                        ValidAudience = Config.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                    },
                    out var validatedToken
                );
                return claims.Claims;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error validating token");
                throw new InvalidJwtException("Error validating token", ex);
            }
        }

        private async Task<IEnumerable<Claim>> VerifyTokenWithJwks(string token, string issuer)
        {
            try
            {
                // Get or create configuration manager for this issuer
                if (!_configurationManagers.TryGetValue(issuer, out var configurationManager))
                {
                    httpClientFactory ??= Provider.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory?.CreateClient() ?? new HttpClient();
                    var metadataAddress = $"{issuer}/.well-known/openid-configuration";
                    configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                        metadataAddress,
                        new OpenIdConnectConfigurationRetriever(),
                        httpClient
                    );
                    _configurationManagers[issuer] = configurationManager;
                }

                // Get the configuration (this will fetch from JWKS if needed)
                var configuration = await configurationManager.GetConfigurationAsync();

                var claims = _handler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeys = configuration.SigningKeys,
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = !string.IsNullOrEmpty(Config.Audience),
                        ValidAudiences = Config.Audiences ?? new[] { Config.Audience },
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                    },
                    out var validatedToken
                );

                return claims.Claims;
            }
            catch (Exception ex)
            {
                logger.Error(ex, new {
                    Message = "Error validating token with JWKS for issuer",
                    Issuer = issuer,
                });
                throw new InvalidJwtException("Error validating token with JWKS for issuer", ex);
            }
        }

        private bool CanUseJwks(string issuer)
        {
            return !string.IsNullOrEmpty(issuer)
                && issuer.StartsWith("https://auth")
                && issuer.EndsWith(".rxnt.com");
        }

        public IEnumerable<Claim> GetClaims(string token)
        {
            try
            {
                var claims = _handler.ReadJwtToken(token);
                return claims.Claims;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error getting claims from token");
                throw new InvalidJwtException("Error getting claims from token", ex);
            }
        }

        public IJwtService Scope(JwtConfig config)
        {
            return new JwtService(config, timeProvider, logger, httpClientFactory);
        }

        public static JwtService Create(
            JwtConfig jwtConfig,
            ILogger<JwtService>? logger = null,
            TimeProvider? timeProvider = null,
            IHttpClientFactory? httpClientFactory = null
        )
        {
            logger ??= new Logger<JwtService>(new LoggerFactory());
            timeProvider ??= TimeProvider.System;
            return new JwtService(jwtConfig, timeProvider, logger, httpClientFactory);
        }
    }
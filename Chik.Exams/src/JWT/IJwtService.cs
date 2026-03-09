using System.Security.Claims;

namespace Chik.Exams;

public interface IJwtService
{
    JwtConfig Config { get; }

    /// <summary>
    /// Generates a JWT token with the provided claims, if the associated JwtConfig contains a Secret
    /// </summary>
    /// <param name="claims">The claims to include in the token</param>
    /// <param name="absoluteExpireTimeUtc">The expiration time for the token. Will use <see cref="RXNT.Common.JWT.JwtConfig.TokenExpiration"/> if omitted.</param>
    /// <returns>The generated token</returns>
    string GenerateToken(
        Dictionary<string, string> claims,
        DateTime? absoluteExpireTimeUtc = null
    );

    /// <summary>
    /// Generates a JWT token with the provided claims, if the associated JwtConfig contains a Secret
    /// </summary>
    /// <param name="claims">The claims to include in the token</param>
    /// <param name="absoluteExpireTimeUtc">The expiration time for the token. Will use <see cref="RXNT.Common.JWT.JwtConfig.TokenExpiration"/> if omitted.</param>
    /// <returns>The generated token</returns>
    string GenerateToken(IEnumerable<Claim> claims, DateTime? absoluteExpireTimeUtc = null);

    /// <summary>
    /// Gets the claims from a JWT token
    /// </summary>
    /// <param name="token">The token to get the claims from</param>
    /// <returns>The claims from the token</returns>
    IEnumerable<Claim> GetClaims(string token);

    /// <summary>
    /// Verifies a JWT token and returns the claims
    /// <para>If the JwtConfig contains an Issuer matching https://auth*.rxnt.com, it will use the JWKS endpoint to verify the token</para>
    /// </summary>
    /// <param name="token">The token to verify</param>
    /// <returns>The claims from the token</returns>
    Task<IEnumerable<Claim>> VerifyToken(string token);

    /// <summary>
    /// <para>Creates a new instance of <see cref="JwtService"/> with the provided <see cref="JwtConfig"/></para>
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    IJwtService Scope(JwtConfig config);
}
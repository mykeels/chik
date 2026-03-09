using Microsoft.IdentityModel.Tokens;

namespace Chik.Exams;

 public record JwtConfig
{
    public const int DefaultTokenExpirationInMinutes = 30;

    public TimeSpan TokenExpiration { get; init; } =
        TimeSpan.FromMinutes(DefaultTokenExpirationInMinutes);

    /// <summary>
    /// <para>The algorithm used to sign the token. Default is <see cref="SecurityAlgorithms.HmacSha256Signature"/></para>
    /// </summary>
    public string Algorithm { get; set; } = SecurityAlgorithms.HmacSha256Signature;

    /// <summary>
    /// The secret key used to sign the token
    /// </summary>
    public string Secret { get; init; } = default!;

    /// <summary>
    /// The issuer of the token, ideally set the current project. e.g. "RXNT.PHR"
    /// If set, it prevents a token that has a different "Issuer" from being verified by this service
    /// </summary>
    public string Issuer { get; init; } = default!;

    /// <summary>
    /// The purpose of the token. e.g. "PasswordReset" or "MagicLink"
    /// If set, it prevents a token that has a different "Audience" from being verified by this service
    /// </summary>
    public string Audience { get; init; } = default!;

    /// <summary>
    /// Contains multiple valid audience values for token validation.
    /// If set, only tokens with an "Audience" claim matching one of these values will be accepted.
    /// </summary>
    public string[] Audiences { get; init; } = default!;

    public SigningCredentials SigningCredentials
    {
        get
        {
            return new SigningCredentials(
                new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Secret)),
                Algorithm
            );
        }
    }
}
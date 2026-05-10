using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using HabitTracker.Api.Options;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HabitTracker.Api.Services.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> jwtOptionsAccessor) : ITokenService
{
    private readonly JwtOptions jwtOptions = jwtOptionsAccessor?.Value ?? throw new ArgumentNullException(nameof(jwtOptionsAccessor));

    public JwtTokenResult CreateAccessToken(string identityUserId, Guid appUserId, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identityUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        ValidateOptions();

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, identityUserId),
            new(ClaimTypes.NameIdentifier, identityUserId),
            new(JwtRegisteredClaimNames.Email, email),
            new("app_user_id", appUserId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return new JwtTokenResult(token, expiresAtUtc);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
        {
            throw new InvalidOperationException("JWT issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
        {
            throw new InvalidOperationException("JWT audience is not configured.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.Key))
        {
            throw new InvalidOperationException("JWT signing key is not configured.");
        }

        if (jwtOptions.Key.Length < 32)
        {
            throw new InvalidOperationException("JWT signing key must be at least 32 characters for HMAC-SHA256.");
        }

        if (jwtOptions.AccessTokenMinutes <= 0)
        {
            throw new InvalidOperationException("JWT access token lifetime must be greater than zero minutes.");
        }
    }
}

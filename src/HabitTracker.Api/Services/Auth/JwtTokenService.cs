using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using HabitTracker.Api.Data;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Options;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HabitTracker.Api.Services.Auth;

public sealed class JwtTokenService(
    IOptions<JwtOptions> jwtOptionsAccessor,
    ApplicationIdentityDbContext identityDbContext) : ITokenService
{
    private readonly JwtOptions jwtOptions = jwtOptionsAccessor?.Value ?? throw new ArgumentNullException(nameof(jwtOptionsAccessor));
    private readonly ApplicationIdentityDbContext identityDbContext = identityDbContext ?? throw new ArgumentNullException(nameof(identityDbContext));

    public JwtTokenResult CreateAccessToken(string identityUserId, Guid appUserId, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identityUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        ValidateOptions();

        var accessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenMinutes);
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
            expires: accessTokenExpiresAtUtc,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        var refreshTokenResult = GenerateRefreshToken(identityUserId);

        return new JwtTokenResult(accessToken, accessTokenExpiresAtUtc, refreshTokenResult.Token, refreshTokenResult.ExpiresAtUtc);
    }

    public RefreshTokenResult GenerateRefreshToken(string identityUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identityUserId);

        var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(7);
        var randomBytes = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        var token = Convert.ToBase64String(randomBytes);

        var refreshToken = new RefreshToken(token, refreshTokenExpiresAtUtc, identityUserId);
        identityDbContext.RefreshTokens.Add(refreshToken);
        identityDbContext.SaveChanges();

        return new RefreshTokenResult(token, refreshTokenExpiresAtUtc);
    }

    public string? ValidateRefreshToken(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var token = identityDbContext.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);

        if (token == null || token.IsExpired)
        {
            return null;
        }

        return token.UserId;
    }

    public void RevokeRefreshToken(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var token = identityDbContext.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);

        if (token != null)
        {
            identityDbContext.RefreshTokens.Remove(token);
            identityDbContext.SaveChanges();
        }
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

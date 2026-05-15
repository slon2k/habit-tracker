namespace HabitTracker.Api.Services.Auth;

public sealed record RefreshTokenResult(string Token, DateTime ExpiresAtUtc);

public interface ITokenService
{
    JwtTokenResult CreateAccessToken(string identityUserId, Guid appUserId, string email);

    // Generates a new refresh token for a user
    RefreshTokenResult GenerateRefreshToken(string identityUserId);

    // Validates a given refresh token and returns the associated user ID if valid
    string? ValidateRefreshToken(string refreshToken, string accessToken);

    // Revokes a refresh token to prevent further use
    void RevokeRefreshToken(string refreshToken);
}
